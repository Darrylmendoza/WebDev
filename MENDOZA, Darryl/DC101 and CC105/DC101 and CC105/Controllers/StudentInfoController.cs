using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using DC101_and_CC105.Data;
using DC101_and_CC105.Models;
using System;
using System.Collections.Generic;

namespace DC101_and_CC105.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentInfoController : Controller
    {
        private readonly Db _db = new Db();

        [HttpGet]
        public IActionResult GetStudents()
        {
            List<Student> students = new List<Student>();
            using var conn = _db.GetConnection();
            conn.Open();
            using var cmd = new MySqlCommand("SELECT * FROM Students ORDER BY StudentNumber ASC", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                students.Add(new Student
                {
                    Number = reader.GetInt32("StudentNumber"),
                    ID = reader.GetInt32("StudentID"),
                    Name = reader.GetString("StudentName"),
                    Grade = reader.GetInt32("StudentGrade"),
                    Section = reader.GetString("StudentSection"),
                    Address = reader.GetString("StudentAddress"),
                    Contact = reader.GetString("StudentContact")
                });
            }
            return Ok(students);
        }

        [HttpPost]
        public IActionResult AddStudent([FromBody] Student student)
        {
            if (string.IsNullOrEmpty(student.Course))
                return BadRequest(new { course = "The course field is required." });

            using var conn = _db.GetConnection();
            conn.Open();

            Random rnd = new Random();
            int newId;
            bool isUnique = false;
            while (!isUnique)
            {
                newId = rnd.Next(1000000, 10000000);
                using var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Students WHERE StudentID=@id", conn);
                checkCmd.Parameters.AddWithValue("@id", newId);
                if (Convert.ToInt32(checkCmd.ExecuteScalar()) == 0)
                {
                    isUnique = true;
                    student.ID = newId;
                }
            }

            string sql = @"INSERT INTO Students (StudentID, StudentName, StudentGrade, StudentSection, StudentAddress, StudentContact)
                           VALUES (@id,@name,@grade,@section,@address,@contact)";
            using var insertCmd = new MySqlCommand(sql, conn);
            insertCmd.Parameters.AddWithValue("@id", student.ID);
            insertCmd.Parameters.AddWithValue("@name", student.Name);
            insertCmd.Parameters.AddWithValue("@grade", student.Grade);
            insertCmd.Parameters.AddWithValue("@section", student.Section.ToUpper());
            insertCmd.Parameters.AddWithValue("@address", student.Address);
            insertCmd.Parameters.AddWithValue("@contact", student.Contact);
            insertCmd.ExecuteNonQuery();

            string courseTable = student.Course.ToUpper() == "BSCS" ? "BSCS" : "ACT";
            string courseSql = $"INSERT INTO {courseTable} (StudentID) VALUES (@id)";
            using var courseCmd = new MySqlCommand(courseSql, conn);
            courseCmd.Parameters.AddWithValue("@id", student.ID);
            courseCmd.ExecuteNonQuery();

            return Ok($"Added successfully. StudentID: {student.ID}");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM Students WHERE StudentID=@id", conn);
            checkCmd.Parameters.AddWithValue("@id", id);
            if (Convert.ToInt32(checkCmd.ExecuteScalar()) == 0)
                return NotFound($"Student with ID {id} not found.");

            using var deleteStudent = new MySqlCommand("DELETE FROM Students WHERE StudentID=@id", conn);
            deleteStudent.Parameters.AddWithValue("@id", id);
            deleteStudent.ExecuteNonQuery();

            using var deleteBscs = new MySqlCommand("DELETE FROM BSCS WHERE StudentID=@id", conn);
            deleteBscs.Parameters.AddWithValue("@id", id);
            deleteBscs.ExecuteNonQuery();

            using var deleteAct = new MySqlCommand("DELETE FROM ACT WHERE StudentID=@id", conn);
            deleteAct.Parameters.AddWithValue("@id", id);
            deleteAct.ExecuteNonQuery();

            return Ok($"Student with ID {id} has been deleted.");
        }

        [HttpGet("courses")]
        public IActionResult GetCourses()
        {
            var bscs = new List<Student>();
            var act = new List<Student>();

            using (var conn = _db.GetConnection())
            {
                conn.Open();

                using (var cmd = new MySqlCommand(
                    "SELECT s.StudentName, s.StudentGrade, s.StudentSection " +
                    "FROM BSCS b " +
                    "JOIN Students s ON b.StudentID = s.StudentID", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bscs.Add(new Student
                        {
                            Name = reader.GetString("StudentName"),
                            Grade = reader.GetInt32("StudentGrade"),
                            Section = reader.GetString("StudentSection")
                        });
                    }
                }

                using (var cmd = new MySqlCommand(
                    "SELECT s.StudentName, s.StudentGrade, s.StudentSection " +
                    "FROM ACT a " +
                    "JOIN Students s ON a.StudentID = s.StudentID", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        act.Add(new Student
                        {
                            Name = reader.GetString("StudentName"),
                            Grade = reader.GetInt32("StudentGrade"),
                            Section = reader.GetString("StudentSection")
                        });
                    }
                }
            }

            return Ok(new { BSCS = bscs, ACT = act });
        }
    }
}
