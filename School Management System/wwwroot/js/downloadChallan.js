// download.js

document.addEventListener("click", function (e) {
    if (e.target && e.target.id === "downloadBtn") {
        console.log("✅ Download button clicked!");
        const element = document.getElementById("printChallan");
        if (element) {
            html2pdf().from(element).save("ChallanForm.pdf");
        } else {
            console.error("❌ printSection not found!");
        }
    }
});



