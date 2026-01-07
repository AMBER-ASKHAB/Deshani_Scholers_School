/* ---------------------- State ---------------------- */
const appState = {
    user: {
        name: "",
        email: "",
        contact: "",
        cnic: "",
        avatar: "https://cdn-icons-png.flaticon.com/512/847/847969.png",
        address: ""
    },
    steps: {
        1: { done: false, data: {} },                // Applying Class
        2: { done: false, data: {} },                // Personal
        3: { done: false, data: { siblings: [] } },  // Education (+ siblings UI lives in 4)
        4: { done: false, data: {} },                // Guardians & Siblings
        5: { done: false, data: {} },                // Guardians & Siblings
        6: { done: false, data: {} }                 // Summary
    }
};
const TOTAL_STEPS = 6;


/* ---------------- Section Switching ---------------- */
function showSection(sectionId) {
    document.querySelectorAll(".page-section").forEach(sec => {
        sec.classList.remove("active");
        sec.style.display = "none";
    });
    const active = document.getElementById(sectionId);
    if (active) {
        active.classList.add("active");
        active.style.display = "block";
    }
    const links = document.querySelectorAll(".sidebar nav ul li");
    links.forEach(li => li.classList.remove("active"));
    const clickedLink = [...links].find(li => {
        const a = li.querySelector("a");
        return a && a.getAttribute("onclick") && a.getAttribute("onclick").includes(sectionId);
    });
    if (clickedLink) clickedLink.classList.add("active");
    if (sectionId === 'viewApplicationSection')
        loadApplication();
}
function loadApplication() {
    fetch('/Applicants/GetApplications')
        .then(res => res.text())
        .then(html => {
            const target = document.querySelector('#viewApplicationSection .card');
            if (target) {
                target.innerHTML = html;
            } else {
                console.error("Target element not found: #viewApplicationSection .card");
            }
        });
}



/* ------------------- Toggle Profile ------------------------ */
function toggleProfile() {
    const el = document.getElementById("profileSidebar");
    if (el) el.classList.toggle("active");
}

/* ------------------- Expandable Menu ------------- */
function toggleExpand(el) {
    if (!el || !el.parentElement) return;
    const parent = el.parentElement;
    parent.classList.toggle("open");
    const content = parent.querySelector(".expandable-content");
    if (content) content.style.display = parent.classList.contains("open") ? "flex" : "none";
}

/* ------------------ Profile init ------------------- */
function initProfile() {
    const get = id => document.getElementById(id);
    const { user } = appState;
    if (get('topbarName')) get('topbarName').textContent = user.name || "";
    if (get('topbarAvatar')) get('topbarAvatar').src = user.avatar;
    if (get('profileAvatar')) get('profileAvatar').src = user.avatar;
    if (get('profileName')) get('profileName').textContent = user.name || "";
    if (get('profileEmail')) get('profileEmail').textContent = user.email || "";
    if (get('profileContact')) get('profileContact').textContent = user.contact || "";
    if (get('profileCNIC')) get('profileCNIC').textContent = user.cnic || "";
}

/* ---------------- Wizard navigation ---------------- */
let currentStep = 1;
let maxStepReached = 1; // furthest step the user has visited (for styling/logic if needed)

// Only green (done) pills are clickable; returning to earlier green steps is allowed.
function canNavigateTo(stepTarget) {
    if (stepTarget === currentStep) return true;
    if (stepTarget == maxStepReached) return true;
    return appState.steps[stepTarget]?.done === true; // green only
}

function countDoneSteps() {
    return Object.keys(appState.steps).reduce((acc, k) => acc + (appState.steps[k].done ? 1 : 0), 0);
}

function goToStep(step) {
    if (step < 1 || step > TOTAL_STEPS) return;
    currentStep = step;
    maxStepReached = Math.max(maxStepReached, step);
    // Show application section
    showSection('applicationSection');

    // Hide all step panes
    document.querySelectorAll('.step-pane').forEach(p => {
        p.classList.add('hidden');
        p.style.display = 'none';
    });

    // Show current step pane and bubble up containers
    const current = document.getElementById('step' + step);
    if (current) {
        current.classList.remove('hidden');
        let el = current;
        while (el) {
            el.style.display = 'block';
            el = el.parentElement;
            if (el && el.classList && el.classList.contains('page-section')) break;
        }
    }

    // Pills & progress
    renderPills('appPills', step);
    renderPills('progressPills', step);
    setProgress('appProgressBar');
    setProgress('progressBar');

    
    // Final step status
    const statusWrap = document.getElementById('applicationStatusWrap');
    if (statusWrap) statusWrap.classList.toggle('hidden', step !== TOTAL_STEPS);

}


/* -------------------- Boot ------------------------- */
window.openCreateApplication = function () {
    showSection('applicationSection');
    goToStep(1);
    setProgress('appProgressBar');
}


window.addEventListener('load', () => {
    initProfile();
    renderPills('appPills', 1);
    setProgress('progressBar');
    setProgress('appProgressBar');
    showSection('dashboardSection');

    // Initial dashboard/detail render + actions
    renderDashboard();
    enableDashboardActions();
    enableDetailsActions();
});

/* --- render step pills --- */
function renderPills(containerId, activeStep) {
    const container = document.getElementById(containerId);
    if (!container) return;
    container.innerHTML = "";

    const labels = ["Applying Class", "Personal", "Education", "Guardians", "Siblings", "Summary"];

    for (let i = 1; i <= TOTAL_STEPS; i++) {
        let stateClass = '';
        if (i === activeStep) {
            stateClass = 'active';                      // red/current
        } else if (appState.steps[i].done) {
            stateClass = 'done';                        // green (completed)
        } else {
            stateClass = 'locked';                      // grey (not completed)
        }

        const pill = document.createElement('button');
        pill.type = 'button';
        pill.className = `step-pill ${stateClass}`;
        pill.setAttribute('data-step', String(i));
        pill.setAttribute('aria-current', i === activeStep ? 'step' : 'false');
        pill.setAttribute('aria-disabled', stateClass === 'locked' ? 'true' : 'false');
        pill.setAttribute('tabindex', stateClass === 'locked' ? '-1' : '0');
        pill.innerHTML = `<span class="num">${i}</span><span class="label">${labels[i - 1]}</span>`;

        pill.addEventListener('click', () => {
            const target = Number(pill.getAttribute('data-step'));
            if (canNavigateTo(target)) goToStep(target);
        });

        container.appendChild(pill);
    }
}

/* ------------------ Helpers ------------------------ */
const val = id => document.getElementById(id)?.value?.trim() || '';
const fileName = id => {
    const el = document.getElementById(id);
    const f = el && el.files && el.files[0];
    return f ? f.name : '';
};
// Select all class chips
var chips = document.querySelectorAll(".class-chip");

// Loop through each chip and add click listener
chips.forEach(chip => {
    chip.addEventListener("click", function () {
        var id = this.getAttribute("data-id");
        var category = this.getAttribute("data-category");
        var description = this.getAttribute("data-description");

        console.log("Selected Class ID:", id);
        console.log("Category:", category);
        console.log("Description:", description);
        document.getElementById("apply_class").value = id;
        // 👉 here you can send id to controller via fetch/ajax if needed
    });
});
/* --------------- Lightbox --------------- */
function openLightbox(src) {
    const lb = document.getElementById('lightbox');
    const img = document.getElementById('lightbox-img');
    if (!lb || !img) return;
    img.src = src;
    lb.style.display = 'flex';
}
function closeLightbox() {
    const lb = document.getElementById('lightbox');
    if (lb) lb.style.display = 'none';
}

/* ---------------- Avatar handling ------------------ */
document.addEventListener('change', e => {
    if (e.target && e.target.id === 'avatarInput') {
        const file = e.target.files && e.target.files[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = ev => {
            const live = document.getElementById('liveAvatar');
            if (live) live.src = ev.target.result;
            appState.user.avatar = ev.target.result;
            initProfile();
        };
        reader.readAsDataURL(file);
    }
});

/* ---------------- Progress bar ---------------- */
function setProgress(barId) {
    const bar = document.getElementById(barId);
    if (!bar) return;
    const done = countDoneSteps();
    const percent = Math.max(0, Math.min(100, Math.round((done / TOTAL_STEPS) * 100)));
    bar.style.width = percent + "%";
}

/* --------------- Save each step data (no validation) --------------- */
var guardianCnicFiles=[];
async function saveStep(step) {
    if (step === 1) {
        const data = {
            appliedForClassId: parseInt(document.getElementById("apply_class").value)
        };

        try {
            const res = await fetch('/Applicants/SaveStep1', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await res.json();
            if (result.success) {
                console.log("✅ Step1 saved, applicantId:", result.applicantId);
                appState.applicantId = result.applicantId;
                console.log("appstate.applicantId:", appState.applicantId);
                appState.steps[1].data = data;
                appState.steps[1].done = true;
                goToStep(2);
            } else {
                alert("❌ Error saving step 1");
            }
        } catch (err) {
            console.error(err);
            alert("Server error");
        }
        return;
    }
    if (step === 2) {
        const formData = new FormData();

        // Always include ApplicantId (comes from Step 1 response)
        formData.append("ApplicantId", appState.applicantId);

        // Personal info fields
        formData.append("CreateApplicant.FullName", val("p_fullname"));
        formData.append("CreateApplicant.Gender", val("p_gender"));
        formData.append("CreateApplicant.DateOfBirth", val("p_dob"));
        formData.append("CreateApplicant.MotherTongue", val("p_mother_tongue"));
        formData.append("CreateApplicant.Contact", val("p_phone"));
        formData.append("CreateApplicant.Email", val("p_email"));
        formData.append("CreateApplicant.BFormNumber", val("p_cnic"));

        // File inputs
        const bFormInput = document.getElementById("fileInput1");
        const profilePicInput = document.getElementById("avatarInput");

        if (bFormInput?.files?.length > 0) {
            formData.append("CreateApplicant.BFormFile", bFormInput.files[0]); // matches model
        }

        if (profilePicInput?.files?.length > 0) {
            formData.append("CreateApplicant.ProfilePic", profilePicInput.files[0]);
        }
        console.log("Submitting ApplicantId:", appState.applicantId);

        try {
            const res = await fetch(`/Applicants/SaveStep2`, {
                method: "POST",
                body: formData
            });

            const result = await res.json();
            if (result.success) {
                console.log("✅ Step 2 saved successfully");
                appState.steps[2].done = true;
                goToStep(3);
            } else {
                alert("❌ Error saving step 2: " + (result.message || ""));
            }
        } catch (err) {
            console.error(err);
            alert(err+" server error in Step 2");
        }
        return;
    }
    if (step === 3) {
        const formData = new FormData();
        // Always include ApplicantId (from step 1 & 2)
        formData.append("ApplicantId", appState.applicantId);

        // Education info fields (match model names)
        formData.append("ApplicantEducation.Category", val("e_level"));
        formData.append("ApplicantEducation.PrevSchool", val("e_school"));
        formData.append("ApplicantEducation.YearsAttended", val("e_years"));
        formData.append("ApplicantEducation.Grade", val("e_grade"));
        formData.append("ApplicantEducation.Percentage", val("e_percentage"));

        // File input for marksheet
        const marksheetInput = document.getElementById("fileInput2");
        if (marksheetInput?.files?.length > 0) {
            formData.append("ApplicantEducation.PreviousSchoolCertid", marksheetInput.files[0]); // must match model property
        }
        const certInput = document.getElementById("fileInput3");
        if (marksheetInput?.files?.length > 0) {
            formData.append("ApplicantEducation.PreviousSchoolLeavCertid", certInput.files[0]); // must match model property
        }

        try {
            const res = await fetch("/Applicants/SaveStep3", {
                method: "POST",
                body: formData
            });

            const result = await res.json();
            if (result.success) {
                console.log("✅ Step 3 saved successfully");
                appState.steps[3].done = true;
                goToStep(4);
            } else {
                alert("❌ Error saving step 3: " + (result.message || ""));
            }
        } catch (err) {
            console.error(err);
            alert("Server error in Step 3");
        }
        return;
    } if (step === 4) {
        const formData = new FormData();

        let grows = document.querySelectorAll("#guardianTable tbody tr");

        grows.forEach((row, i) => {
            let cells = row.querySelectorAll("td");

            formData.append(`Guardians[${i}].GuardName`, cells[0].innerText.trim());
            formData.append(`Guardians[${i}].GuardRelation`, cells[1].innerText.trim());
            formData.append(`Guardians[${i}].CNIC`, cells[2].innerText.trim());
            formData.append(`Guardians[${i}].Contact`, cells[3].innerText.trim());
            formData.append(`Guardians[${i}].Occupation`, cells[4].innerText.trim());
            formData.append(`Guardians[${i}].GuardGender`, cells[5].innerText.trim());
            formData.append(`Guardians[${i}].Address`, cells[6].innerText.trim());
            formData.append(`Guardians[${i}].Email`, cells[7].innerText.trim());

            // CNIC files
            if (guardianCnicFiles[i]?.CnicFront) {
                formData.append(`Guardians[${i}].CnicFront`, guardianCnicFiles[i].CnicFront);
            }
            if (guardianCnicFiles[i]?.CnicBack) {
                formData.append(`Guardians[${i}].CnicBack`, guardianCnicFiles[i].CnicBack);
            }
        });

        // Siblings


        // ApplicantId
        formData.append("ApplicantId", appState.applicantId);

        try {
            const res = await fetch("/Applicants/SaveStep4", {
                method: "POST",
                body: formData
            });

            const result = await res.json();
            if (result.success) {
                console.log("✅ Step 4 saved successfully");
                appState.steps[4].done = true;
                loadSummary(appState.applicantId);
                goToStep(5);
            } else {
                alert("❌ Error saving step 4: " + (result.message || ""));
            }
        } catch (err) {
            console.error(err);
            alert("Server error in Step 4");
        }
    }
    if (step === 5)
    {
        let formData = new FormData();
        let srows = document.querySelectorAll("#siblingsTable tbody tr");
        srows.forEach((row, i) => {
            let cells = row.querySelectorAll("td");

            formData.append(`Siblings[${i}].Name`, cells[0].innerText.trim());
            formData.append(`Siblings[${i}].ClassText`, cells[1].innerText.trim());
            formData.append(`Siblings[${i}].BForm`, cells[2].innerText.trim());
        });
        // ApplicantId
        formData.append("ApplicantId", appState.applicantId);

        try {
            const res = await fetch("/Applicants/SaveStep5", {
                method: "POST",
                body: formData
            });

            const result = await res.json();
            if (result.success) {
                console.log("✅ Step 5 saved successfully");
                appState.steps[5].done = true;
                loadSummary(appState.applicantId);
                goToStep(6);
            } else {
                alert("❌ Error saving step 5: " + (result.message || ""));
            }
        } catch (err) {
            console.error(err);
            alert("Server error in Step 5");
        }
    }
    if (step === 6) {
        try {
            const formData = new FormData();
            formData.append("ApplicantId", appState.applicantId);

            console.log("📤 Submitting ApplicantId (step 6):", appState.applicantId);

            const res = await fetch(`/Applicants/SubmitApplication`, {
                method: "POST",
                body: formData
            });

            if (!res.ok) {
                throw new Error(`HTTP error! Status: ${res.status}`);
            }

            const result = await res.json();
            if (result.success) {
                console.log("✅ Application submitted successfully");
                alert("✅ Application submitted successfully!");
                showSection('viewApplicationSection');
            } else {
                console.warn("❌ Server rejected:", result.message);
                alert("❌ Error submitting application: " + (result.message || ""));
            }

            appState.steps[6].done = true;
        } catch (err) {
            console.error("🚨 Submit Step 6 Error:", err);
            alert("Server error during submission. Check console for details.");
        }
    }
    async function loadSummary(applicantId) {
        try {
            const res = await fetch(`/Applicants/ApplicationSummary?applicantId=${applicantId}`);
            if (res.ok) {
                const html = await res.text();
                document.querySelector("#applicationDetails tbody").innerHTML = html;
            } else {
                console.error("Error loading summary", res.statusText);
            }
        } catch (err) {
            console.error("Server error", err);
        }
    }

    // Update progress immediately
    setProgress('appProgressBar');
    setProgress('progressBar');

    // Move forward ONLY if there are further steps
    if (step < TOTAL_STEPS) {
        maxStepReached = Math.max(maxStepReached, step + 1);
        goToStep(step + 1);
    } else {
        // Stay on summary; re-render pills to reflect all done
        renderPills('appPills', step);
        renderPills('progressPills', step);
    }
}

function toggleEduFields() {
    const levelEl = document.getElementById("e_level");
    const extraFields = document.getElementById("eduExtraFields");
    if (!levelEl || !extraFields) return;

    const level = levelEl.value;
    if (level > 2) {
        extraFields.style.display = "flex";
        extraFields.style.flexWrap = "wrap";
        extraFields.style.gap = "10px";
    } else {
        extraFields.style.display = "none";
    }
}

/* ----------------- Siblings ----------------- */
let editSiblingRow = null; // Track editing row

function showSiblingForm() {
    const el = document.getElementById("sibling");
    if (el) el.style.display = "flex";
}

async function saveSibling() {
    const bFormInput = document.getElementById("s_bform");
    const nameInput = document.getElementById("s_name");
    const classInput = document.getElementById("s_class");
    const saveBtn = document.getElementById("s_saveBtn");

    const bForm = bFormInput?.value.trim();
    const name = nameInput?.value.trim();
    const sClass = classInput?.value.trim();

    if (!bForm || !name || !sClass) {
        alert("Please fill all sibling fields.");
        return;
    }

    try {
        // placeholder for future backend call
        const data = { name, class: sClass, bform: bForm };

        if (editSiblingRow) {
            // Update row
            editSiblingRow.cells[0].innerText = data.name;
            editSiblingRow.cells[1].innerText = data.class;
            editSiblingRow.cells[2].innerText = data.bform;
            editSiblingRow = null;
            if (saveBtn) saveBtn.innerText = "Save"; // reset button
        } else {
            // Add new row
            const table = document.getElementById("siblingsTable")?.querySelector("tbody");
            if (table) {
                const row = table.insertRow();
                row.innerHTML = `
          <td>${data.name}</td>
          <td>${data.class}</td>
          <td>${data.bform}</td>
          <td>
            <button class="btn yellow" onclick="editSibling(this)">Edit</button>
            <button class="btn red" onclick="this.closest('tr').remove()">Delete</button>
          </td>
        `;             
            }
        }

        // Reset form
        clearSiblingForm();
    } catch (err) {
        console.error(err);
        alert("Error fetching sibling data.");
    }
}

function editSibling(button) {
    editSiblingRow = button?.closest("tr");
    if (!editSiblingRow) return;

    const sName = document.getElementById("s_name");
    const sClass = document.getElementById("s_class");
    const sBform = document.getElementById("s_bform");
    const saveBtn = document.getElementById("s_saveBtn");

    if (sName) sName.value = editSiblingRow.cells[0].innerText;
    if (sClass) sClass.value = editSiblingRow.cells[1].innerText;
    if (sBform) sBform.value = editSiblingRow.cells[2].innerText;

    if (saveBtn) saveBtn.innerText = "Update";
    showSiblingForm();
}

function clearSiblingForm() {
    const s = id => document.getElementById(id);
    if (s("s_name")) s("s_name").value = "";
    if (s("s_class")) s("s_class").value = "";
    if (s("s_bform")) s("s_bform").value = "";
    const holder = s("sibling");
    if (holder) holder.style.display = "none";
}

function readSiblings() {
    const rows = [...document.querySelectorAll('#siblingsTable tbody tr')];
    return rows.map(r => ({
        name: r.cells[0]?.textContent?.trim() || '',
        class: r.cells[1]?.textContent?.trim() || '',
        bform: r.cells[2]?.textContent?.trim() || ''
    })).filter(x => x.name || x.class || x.bform);
}

/* --------------- Fee structure logic --------------- */
document.addEventListener('change', e => {
    if (e.target && e.target.id === 'f_class') {
        const cls = e.target.value;
        const fee = classFeeMap[cls] || null;
        const target = document.getElementById('f_structure');
        if (target) target.value = fee ? `Admission: ${fee.admission}, Monthly: ${fee.monthly}` : '';
    }
});

const classFeeMap = {
    "Nursery": { admission: "Rs 5,000", monthly: "Rs 2,000" },
    "KG": { admission: "Rs 5,000", monthly: "Rs 2,200" },
    "1": { admission: "Rs 6,000", monthly: "Rs 2,500" },
    "2": { admission: "Rs 6,000", monthly: "Rs 2,700" },
    "3": { admission: "Rs 6,500", monthly: "Rs 2,900" },
    "4": { admission: "Rs 7,000", monthly: "Rs 3,100" },
    "5": { admission: "Rs 7,500", monthly: "Rs 3,300" },
    "6": { admission: "Rs 8,000", monthly: "Rs 3,600" },
    "7": { admission: "Rs 8,500", monthly: "Rs 3,900" },
    "8": { admission: "Rs 9,000", monthly: "Rs 4,200" },
    "9": { admission: "Rs 10,000", monthly: "Rs 4,800" },
    "10": { admission: "Rs 12,000", monthly: "Rs 5,300" }
};

function generateChallan() {
    // Pull name from Step 2 (personal), class from Step 1 (applying class)
    const name = appState.steps[2]?.data?.fullname || appState.user.name || "-";
    const cls =
        document.getElementById('f_class')?.value ||
        appState.steps[1]?.data?.applyClass ||
        "-";
    const fee = classFeeMap[cls] || { admission: "-", monthly: "-" };
    const html = `
    <div><strong>Student:</strong> ${name}</div>
    <div><strong>Class:</strong> ${cls}</div>
    <div><strong>Admission Fee:</strong> ${fee.admission}</div>
    <div><strong>Monthly Fee:</strong> ${fee.monthly}</div>
    <div><strong>Due Date:</strong> 20 Aug 2025</div>
    <hr/>
    <div class="small">* Present this challan at the designated bank branch.</div>
  `;
    const body = document.getElementById('challanBody');
    const preview = document.getElementById('challanPreview');
    if (body) body.innerHTML = html;
    if (preview) preview.classList.remove('hidden');
}

/* --- Expand Application Sidebar Highlight --- */
function openApplication(el) {
    const li = el?.parentElement;
    if (!li) return;
    document.querySelectorAll(".expandable").forEach(item => {
        if (item !== li) item.classList.remove("open");
    });
    li.classList.add("open");
    el.classList.add("active");
}

// Highlight active inner links
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".expandable .expandable-content a").forEach(link => {
        link.addEventListener("click", function () {
            document.querySelectorAll(".expandable .expandable-content a").forEach(l => l.classList.remove("active"));
            this.classList.add("active");
        });
    });
});

/* --------------- Applying for Class -------------------- */
document.addEventListener("DOMContentLoaded", () => {
    console.log("Script loaded!");
    const quickLinksData = window.quickLinksData || {};
    const categoriesContainer = document.querySelector(".categories");
    const applyInput = document.getElementById("apply_class");
    const quickLinksList = document.getElementById("quickLinksList");
    const quickLinksBox = document.getElementById("quickLinks");

    if (!categoriesContainer) {
        console.error("❌ No .categories element found!");
        return;
    }

    // EVENT DELEGATION: handle clicks on categories & class chips
    categoriesContainer.addEventListener("click", e => {
        // --- Class chip clicked ---
        const chip = e.target.closest(".class-chip");
        if (chip) {
            console.log("✅ Class selected:", chip.dataset.id, chip.dataset.description);

            // Highlight selected class
            document.querySelectorAll(".class-chip").forEach(c => c.classList.remove("selected"));
            chip.classList.add("selected");

            // Save selected class ID in hidden input
            if (applyInput) applyInput.value = chip.dataset.id;

            // Show Quick Links dynamically
            const selectedClassId = chip.dataset.id;
            const quickLinks = quickLinksData[selectedClassId] || [];

            if (quickLinksList) quickLinksList.innerHTML = "";
            if (quickLinks.length > 0 && quickLinksList && quickLinksBox) {
                quickLinks.forEach(link => {
                    const li = document.createElement("li");
                    li.innerHTML = `<a href="${link.Url}" target="_blank">${link.Title}</a>`;
                    quickLinksList.appendChild(li);
                });
                quickLinksBox.style.display = "block";
            } else if (quickLinksBox) {
                quickLinksBox.style.display = "none";
            }

            // Highlight parent category
            document.querySelectorAll(".category-card").forEach(c => c.classList.remove("active"));
            const parentCard = chip.closest(".category-card");
            if (parentCard) parentCard.classList.add("active");

            return; // prevent also triggering category click
        }

        // --- Category card clicked (if not on a chip) ---
        const card = e.target.closest(".category-card");
        if (card) {
            console.log("📂 Category selected:", card.dataset.group);
            document.querySelectorAll(".category-card").forEach(c => c.classList.remove("active"));
            card.classList.add("active");
        }
    });
});



// ========================== Guardians Table
let editRow = null; // Track which row is being edited

function showGuardianForm() {
    const form = document.getElementById("guard_form");
    if (form) form.style.display = "block";
}

function addGuardianRow() {
    const get = id => document.getElementById(id);
    const name = get("g_name")?.value.trim();
    const relation = get("g_relation")?.value;
    const cnic = get("g_cnic")?.value.trim();
    const contact = get("g_contact")?.value.trim();
    const job = get("g_job")?.value.trim();
    const gender = get("g_gender")?.value?.trim();
    const address = get("g_address")?.value?.trim();
    const email = get("g_email")?.value?.trim();
    const saveBtn = document.querySelector("#guard_form button");
    const cnicFront = document.getElementById("g_cnic_front");
    const cnicBack = document.getElementById("g_cnic_back");
    guardianCnicFiles.push({
        CnicFront: cnicFront.files[0] || null,
        CnicBack: cnicBack.files[0] || null
    });

    if (!name || !relation || !cnic || !contact || !job || !gender || !address || !email) {
        alert("Please fill all guardian fields.");
        return;
    }
    if (editRow) {
        // Update existing row
        editRow.cells[0].innerText = name;
        editRow.cells[1].innerText = relation;
        editRow.cells[2].innerText = cnic;
        editRow.cells[3].innerText = contact;
        editRow.cells[4].innerText = job;
        editRow.cells[5].innerText = gender;
        editRow.cells[6].innerText = address;
        editRow.cells[7].innerText = email;                

        editRow = null;
        if (saveBtn) saveBtn.innerText = "Save"; // Switch button text back
    } else {
        // Add new row
        const table = document.getElementById("guardianTable")?.querySelector("tbody");
        if (table) {
            const row = table.insertRow();
            row.innerHTML = `
        <td>${name}</td>
        <td>${relation}</td>
        <td>${cnic}</td>
        <td>${contact}</td>
        <td>${job}</td>
        <td>${gender}</td>
        <td>${address}</td>
        <td>${email}</td>
        <td>
          <button class="btn yellow" onclick="editGuardianRow(this)">Edit</button>
          <button class="btn red" onclick="this.parentElement.parentElement.remove()">Delete</button>
        </td>
      `;           
        }
    }

    clearGuardianForm();
}

function editGuardianRow(button) {
    editRow = button?.parentElement?.parentElement || null; // Get row reference
    if (!editRow) return;

    // Fill back values into form
    const get = id => document.getElementById(id);
    if (get("g_name")) get("g_name").value = editRow.cells[0].innerText;
    if (get("g_relation")) get("g_relation").value = editRow.cells[1].innerText;
    if (get("g_cnic")) get("g_cnic").value = editRow.cells[2].innerText;
    if (get("g_contact")) get("g_contact").value = editRow.cells[3].innerText;
    if (get("g_job")) get("g_job").value = editRow.cells[4].innerText;
    if (get("g_gender")) get("g_gender").value = editRow.cells[5].innerText;
    if (get("g_address")) get("g_address").value = editRow.cells[6].innerText;
    if (get("g_email")) get("g_email").value = editRow.cells[7].innerText;

    // Change button text to "Update"
    const saveBtn = document.querySelector("#guard_form button");
    if (saveBtn) saveBtn.innerText = "Update";

    // Show form
    showGuardianForm();
}

function clearGuardianForm() {
    const ids = ["g_name", "g_relation", "g_cnic", "g_contact", "g_job", "g_gender", "g_address", "g_email"];
    ids.forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = "";
    });
}

/* ================= Dashboard Actions ================= */
function renderDashboard() {
    const tbody = document.querySelector("#applicationDashboard tbody");
    if (!tbody) return;

    tbody.innerHTML = "";

    dashboardData.forEach(row => {
        const tr = document.createElement("tr");
        tr.innerHTML = `
            <td>${row.applicantId}</td>
            <td>${row.applicationName}</td>
            <td>${row.programName}</td>
            <td><span class="status-badge in-progress">${row.status}</span></td>
            <td>
                <button class="btn btn-sm btn-info"><i class="fa fa-eye"></i></button>
                <button class="btn btn-sm btn-warning"><i class="fa fa-edit"></i></button>
                <button class="btn btn-sm btn-danger"><i class="fa fa-trash"></i></button>
            </td>
        `;
        tbody.appendChild(tr);
    });
}

window.enableDashboardActions = function () {
    const dashboard = document.getElementById("applicationDashboard");
    if (!dashboard) return;

    dashboard.querySelectorAll("tbody tr").forEach(row => {
        const applicantId = row.cells[0]?.textContent?.trim();

        const viewBtn = row.querySelector(".btn-info");
        if (viewBtn) {
            viewBtn.addEventListener("click", () => {
                if (!applicantId) return;
                showSection('viewApplicationSection');
                // loadApplicationDetails(applicantId); // hook here
            });
        }

        const editBtn = row.querySelector(".btn-warning");
        if (editBtn) {
            editBtn.addEventListener("click", () => {
                if (!applicantId) return;
                showSection('applicationSection');
                goToStep(1);
                // loadApplicationForEdit(applicantId); // hook here
            });
        }

        const delBtn = row.querySelector(".btn-danger");
        if (delBtn) {
            delBtn.addEventListener("click", () => {
                row.remove();
                // optionally: fetch(`/Applicants/Delete?id=${applicantId}`, { method: 'DELETE' })
            });
        }
    });
};

/* ================= Dashboard Actions ================= */
window.enableDashboardActions = function () {
    const dashboard = document.getElementById("applicationDashboard");
    if (!dashboard) return;

    dashboard.querySelectorAll("tbody tr").forEach(row => {
        const applicantId = row.cells[0]?.textContent?.trim();

        const viewBtn = row.querySelector(".btn-info");
        if (viewBtn) {
            viewBtn.addEventListener("click", () => {
                if (!applicantId) return;
                showSection('viewApplicationSection');
                // loadApplicationDetails(applicantId); // hook here
            });
        }

        const editBtn = row.querySelector(".btn-warning");
        if (editBtn) {
            editBtn.addEventListener("click", () => {
                if (!applicantId) return;
                showSection('applicationSection');
                goToStep(1);
                // loadApplicationForEdit(applicantId); // hook here
            });
        }

        const delBtn = row.querySelector(".btn-danger");
        if (delBtn) {
            delBtn.addEventListener("click", () => {
                row.remove();
                // optionally: fetch(`/Applicants/Delete?id=${applicantId}`, { method: 'DELETE' })
            });
        }
    });
};

/* ================= Details Actions ================= */
window.enableDetailsActions = function () {
    const details = document.getElementById("applicationDetails");
    if (!details) return;

    details.querySelectorAll("tbody tr").forEach(row => {
        const stepNum = Number(row.cells[0]?.textContent);

        const viewBtn = row.querySelector(".btn-info");
        if (viewBtn) {
            viewBtn.addEventListener("click", () => {
                if (!stepNum) return;
                showSection('applicationSection');
                goToStep(stepNum);
                // optionally: make fields read-only
            });
        }

        const editBtn = row.querySelector(".btn-warning");
        if (editBtn) {
            editBtn.addEventListener("click", () => {
                if (!stepNum) return;
                showSection('applicationSection');
                goToStep(stepNum);
            });
        }
    });
};

/* ---------------- File uploads (distinct targets) ---------------- */
// Step 2 file upload
(() => {
    const fileInput1 = document.getElementById("fileInput1");
    const fileList1 = document.getElementById("fileList1") || document.getElementById("fileList");
    if (fileInput1 && fileList1) {
        fileInput1.addEventListener("change", (event) => {
            const file = event.target.files && event.target.files[0];
            if (!file) return;
            const fileURL = URL.createObjectURL(file);
            const fileItem = document.createElement("div");
            fileItem.classList.add("file-item");
            fileItem.innerHTML = `
        <span>${file.name}</span>
        <button class="open-btn" type="button">Open</button>
      `;
            fileItem.querySelector(".open-btn").addEventListener("click", () => {
                window.open(fileURL, "_blank");
            });
            fileList1.innerHTML = "";
            fileList1.appendChild(fileItem);
        });
    }
})();

// Step 3 file upload
// Step 3 file upload
(() => {
    const fileInput2 = document.getElementById("fileInput2");
    const fileList2 = document.getElementById("fileList2") || document.getElementById("fileList");
    if (fileInput2 && fileList2) {
        fileInput2.addEventListener("change", (event) => {
            const file = event.target.files && event.target.files[0];
            if (!file) return;
            const fileURL = URL.createObjectURL(file);
            const fileItem = document.createElement("div");
            fileItem.classList.add("file-item");
            fileItem.innerHTML = `
        <span>${file.name}</span>
        <button class="open-btn" type="button">Open</button>
      `;
            fileItem.querySelector(".open-btn").addEventListener("click", () => {
                window.open(fileURL, "_blank");
            });
            fileList2.innerHTML = "";
            fileList2.appendChild(fileItem);
        });
    }
})();

// Keep action enablement in sync if tables are re-rendered dynamically later
document.addEventListener("DOMContentLoaded", () => {
    if (typeof window.enableDashboardActions === "function") {
        window.enableDashboardActions();
    }
    if (typeof window.enableDetailsActions === "function") {
        window.enableDetailsActions();
    }
});

