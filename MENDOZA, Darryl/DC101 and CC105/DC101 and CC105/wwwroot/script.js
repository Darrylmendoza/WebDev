const studentList = document.getElementById("studentList");
const bscsList = document.getElementById("bscsList");
const actList = document.getElementById("actList");
const addBtn = document.getElementById("addStudentBtn");
const deleteBtn = document.getElementById("deleteStudentBtn");

const nameInput = document.getElementById("name");
const gradeInput = document.getElementById("grade");
const sectionInput = document.getElementById("section");
const addressInput = document.getElementById("address");
const contactInput = document.getElementById("contact");
const courseSelect = document.getElementById("course");
const deleteIdInput = document.getElementById("deleteId");
const addedIdMessage = document.getElementById("addedIdMessage");

async function fetchStudents() {
    const res = await fetch("/api/StudentInfo");
    const students = await res.json();
    studentList.innerHTML = "";
    students.forEach(s => {
        const row = document.createElement("div");
        row.className = "listview-row";
        row.innerHTML = `
            <div>${s.id}</div>
            <div>${s.name}</div>
            <div>${s.grade}</div>
            <div>${s.section}</div>
            <div>${s.address}</div>
            <div>${s.contact}</div>
        `;
        studentList.appendChild(row);
    });

    const resCourses = await fetch("/api/StudentInfo/courses");
    const courses = await resCourses.json();
    bscsList.innerHTML = "";
    actList.innerHTML = "";

    courses.bscs.forEach(s => {
        const row = document.createElement("div");
        row.className = "listview-row-small";
        row.innerHTML = `
        <div>${s.name}</div>
        <div>${s.grade}</div>
        <div>${s.section}</div>
    `;
        bscsList.appendChild(row);
    });

    courses.act.forEach(s => {
        const row = document.createElement("div");
        row.className = "listview-row-small";
        row.innerHTML = `
        <div>${s.name}</div>
        <div>${s.grade}</div>
        <div>${s.section}</div>
    `;
        actList.appendChild(row);
    });



}

addBtn.addEventListener("click", async () => {
    const student = {
        Name: nameInput.value.trim(),
        Grade: parseInt(gradeInput.value),
        Section: sectionInput.value.trim(),
        Address: addressInput.value.trim(),
        Contact: contactInput.value.trim(),
        Course: courseSelect.value
    };

    if (!student.Name || !student.Grade || !student.Section || !student.Address || !student.Contact || !student.Course) {
        alert("Some required fields are empty");
        return;
    }

    const res = await fetch("/api/StudentInfo", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(student)
    });

    const resultText = await res.text();
    if (!res.ok) {
        alert("Error: " + resultText);
        return;
    }

    addedIdMessage.textContent = resultText;
    nameInput.value = "";
    gradeInput.value = "";
    sectionInput.value = "";
    addressInput.value = "";
    contactInput.value = "";
    courseSelect.value = "";

    fetchStudents();
});

deleteBtn.addEventListener("click", async () => {
    const id = deleteIdInput.value.trim();
    if (!id) {
        alert("Enter Student ID to delete");
        return;
    }

    const res = await fetch(`/api/StudentInfo/${id}`, { method: "DELETE" });
    const resultText = await res.text();
    if (!res.ok) {
        alert("Error: " + resultText);
        return;
    }

    alert(resultText);
    deleteIdInput.value = "";
    fetchStudents();
});

window.addEventListener("load", fetchStudents);
