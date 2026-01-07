// download.js

document.addEventListener("click", function (e) {
    if (e.target && e.target.id === "downloadBtn") {
        console.log("✅ Download button clicked!");
        const element = document.getElementById("printSection");
        if (element) {
            html2pdf().from(element).save("AdmissionForm.pdf");
        } else {
            console.error("❌ printSection not found!");
        }
    }
});


document.addEventListener("click", function (e) {
    if (e.target && e.target.id === "viewFormBtn") {
        const form = document.getElementById("submittedForm");
        const btn = e.target;

        if (form) {
            if (form.style.display === "none" || form.style.display === "") {
                form.style.display = "block";
                btn.innerText = "Hide Submitted Application Form";
            } else {
                form.style.display = "none";
                btn.innerText = "View Submitted Application Form";
            }
        }
    }
});
