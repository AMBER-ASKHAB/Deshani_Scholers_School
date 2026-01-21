document.addEventListener("click", function (e) {

    if (e.target && e.target.id === "downloadBtn") {

        console.log("✅ Download button clicked!");

        const element = document.getElementById("printSection");

        if (!element) {
            console.error("❌ printSection not found!");
            return;
        }

        html2pdf(element, {
            margin: 10,
            filename: "AdmissionForm.pdf",
            image: { type: "jpeg", quality: 0.95 },
            html2canvas: { scale: 2 },
            jsPDF: { unit: "mm", format: "a4", orientation: "portrait" }
        });
    }
});
