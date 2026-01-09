// ========== ADMIN PANEL UNIFIED JAVASCRIPT ==========

// ========== GLOBAL VARIABLES ==========
let editingSubjectId = null;
let isSubjectFormSubmitting = false;
let isClassFormSubmitting = false;
let isSectionFormSubmitting = false;

// ==================== GLOBAL FUNCTIONS ====================

// Load classes dynamically for Class Fee section
function loadClassesForClassFee(selectedCategory = null) {
    const classDropdown = document.getElementById('classID');
    const tableClassDropdown = document.getElementById('tableclassID');

    console.log('loadClassesForClassFee called. Category:', selectedCategory);
    console.log('classDropdown found:', !!classDropdown);
    console.log('tableClassDropdown found:', !!tableClassDropdown);

    if (!classDropdown && !tableClassDropdown) {
        console.warn('Neither class dropdown found, retrying in 100ms...');
        return new Promise((resolve) => {
            setTimeout(() => {
                loadClassesForClassFee(selectedCategory).then(resolve).catch(resolve);
            }, 100);
        });
    }

    const dropdownsToUpdate = [classDropdown, tableClassDropdown].filter(dd => dd !== null);

    if (dropdownsToUpdate.length === 0) {
        console.error('No valid dropdowns to update');
        return Promise.resolve();
    }

    console.log('Fetching classes from server...');

    return fetch('/Admin/AdminDashboard/GetClasses')
        .then(response => {
            console.log('Response status:', response.status);
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
            return response.json();
        })
        .then(data => {
            console.log('Classes fetched:', data);
            console.log('Data type:', Array.isArray(data) ? 'Array' : typeof data);
            console.log('Data length:', Array.isArray(data) ? data.length : 'N/A');

            // API returns { Id, Description } - use all classes (no category filtering needed)
            const filteredClasses = Array.isArray(data) ? data : [];

            if (filteredClasses.length === 0) {
                console.warn('No classes returned from API');
            }

            dropdownsToUpdate.forEach(dd => {
                if (!dd) return;

                // Clear existing options and add default
                dd.innerHTML = '';
                const defaultOpt = document.createElement('option');
                defaultOpt.value = '';
                defaultOpt.textContent = '--Select--';
                dd.appendChild(defaultOpt);

                // Populate new options - handle both Id/Description and id/description
                filteredClasses.forEach(c => {
                    const opt = document.createElement('option');
                    const classId = c.Id !== undefined ? c.Id : (c.id !== undefined ? c.id : null);
                    const classDesc = c.Description !== undefined ? c.Description : (c.description !== undefined ? c.description : 'Unknown');

                    if (classId !== null) {
                        opt.value = classId;
                        opt.textContent = classDesc;
                        dd.appendChild(opt);
                    }
                });
            });

            console.log('Class dropdowns populated successfully. Count:', filteredClasses.length);
        })
        .catch(err => {
            console.error('Error loading classes:', err);
            dropdownsToUpdate.forEach(dd => {
                if (!dd) return;
                dd.innerHTML = '';
                const opt = document.createElement('option');
                opt.disabled = true;
                opt.textContent = 'Error loading classes';
                dd.appendChild(opt);
            });
        });
}

// Load fee heads dynamically for Class Fee section
function loadFeeHeadsForClassFee(category) {
    const feeHeadSelect = document.getElementById('feeHeadS');
    if (!feeHeadSelect) {
        console.warn('feeHeadS dropdown not found');
        return Promise.resolve();
    }

    console.log('Fetching fee heads for category:', category);

    return fetch(`/Admin/AdminDashboard/GetFeeHeads?category=${encodeURIComponent(category)}`)
        .then(res => {
            if (!res.ok) throw new Error(`HTTP error! Status: ${res.status}`);
            return res.json();
        })
        .then(data => {
            console.log('Fee heads fetched:', data);

            feeHeadSelect.innerHTML = '';
            const defaultOpt = document.createElement('option');
            defaultOpt.value = '';
            defaultOpt.textContent = '--Select--';
            feeHeadSelect.appendChild(defaultOpt);

            if (data && Array.isArray(data) && data.length > 0) {
                data.forEach(f => {
                    const opt = document.createElement('option');
                    opt.value = f.id;
                    opt.textContent = f.feeHEAD;
                    feeHeadSelect.appendChild(opt);
                });
            } else {
                console.warn('No fee heads returned or data is not an array');
            }
        })
        .catch(err => {
            console.error('Error loading fee heads:', err);
            feeHeadSelect.innerHTML = '';
            const opt = document.createElement('option');
            opt.disabled = true;
            opt.textContent = 'Error loading fee heads';
            feeHeadSelect.appendChild(opt);
        });
}

// ==================== DOM CONTENT LOADED ====================

document.addEventListener('DOMContentLoaded', () => {
    const categorySelect = document.getElementById('classFeeCategory');
    const classField = document.getElementById('classField');

    if (!categorySelect || !classField) return;

    // Hide class field initially
    classField.style.display = 'none';

    categorySelect.addEventListener('change', () => {
        const selectedCategory = categorySelect.value;

        if (!selectedCategory) {
            classField.style.display = 'none';
            return;
        }

        // Show class dropdown and load data
        classField.style.display = 'block';
        loadClassesForClassFee(selectedCategory);
        loadFeeHeadsForClassFee(selectedCategory);
    });
});
// This file handles all workspace switching and functionality

document.addEventListener('DOMContentLoaded', () => {
    initializeAdminPanel();
});

// ========== WORKSPACE MANAGEMENT ==========
function initializeAdminPanel() {
    setupSidebarNavigation();
    setupCollapsibleMenus();
    initializeFeeManagement();
    initializeClassFeeManagement();
    initializeClassManagement();
    initializeSectionManagement()
    initializeStaffManagement();
    initializeChallanManagement();
    initializeSubject();
    initClassTimeTable();
}

// Switch workspace section
function switchWorkspace(sectionId) {
    // Hide all sections
    document.querySelectorAll('.workspace-section').forEach(section => {
        section.classList.remove('active');
    });

    // Show selected section
    const targetSection = document.getElementById(sectionId);
    if (targetSection) {
        targetSection.classList.add('active');
    } else {
        // Fallback to welcome
        document.getElementById('welcomeSection').classList.add('active');
    }

    // Update active menu items
    document.querySelectorAll('.submenu-item').forEach(item => {
        item.classList.remove('active');
    });
    document.querySelectorAll('.sidebar-item').forEach(item => {
        item.classList.remove('active');
    });

    // Activate the clicked menu item
    const clickedItem = document.querySelector(`[data-section="${sectionId}"]`);
    if (clickedItem) {
        clickedItem.closest('.submenu-item')?.classList.add('active');
        clickedItem.closest('.sidebar-item')?.classList.add('active');
    }

    // Initialize section-specific functionality
    initializeSection(sectionId);
}

// Initialize section-specific functionality
function initializeSection(sectionId) {
    switch (sectionId) {
        case 'feeSetup':
            loadFees();
            break;
        case 'classFee':
            loadClassesForClassFee();
            break;
        case 'addSection':
            loadClassesForSection();
            break;
        case 'allocateTeacher':
            loadClassesForSectionTeachers();
            break;
        case 'viewStaff':
            loadStaffTable();
            break;
        case 'addEducation':
            loadLatestStaff();
            break;
        case 'addSubject':
            loadClassesforSubjects();
            break;
    }
}

// ========== SIDEBAR NAVIGATION ==========
function setupSidebarNavigation() {
    // Handle all menu items with data-section attribute
    document.querySelectorAll('[data-section]').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const sectionId = link.getAttribute('data-section');
            switchWorkspace(sectionId);
        });
    });

    // Handle Dashboard link specifically
    const dashboardLink = document.querySelector('.dashboard-link');
    if (dashboardLink) {
        dashboardLink.addEventListener('click', (e) => {
            e.preventDefault();
            switchWorkspace('welcomeSection');
        });
    }
}

// ========== COLLAPSIBLE MENUS ==========
function setupCollapsibleMenus() {
    const menus = document.querySelectorAll('.collapsible');

    function closeAllMenus() {
        menus.forEach(m => {
            m.querySelector('.submenu')?.classList.remove('open');
            m.querySelector('.indicator')?.classList.remove('rotate');
        });
    }

    function openMenu(key) {
        const menu = document.querySelector(`.collapsible[data-menu="${key}"]`);
        if (!menu) return;
        menu.querySelector('.submenu')?.classList.add('open');
        menu.querySelector('.indicator')?.classList.add('rotate');
    }

    // Auto-open menu based on active section
    document.querySelectorAll('.collapse-toggle').forEach(btn => {
        btn.addEventListener('click', () => {
            const parent = btn.closest('.collapsible');
            const key = parent.dataset.menu;
            const isOpen = parent.querySelector('.submenu')?.classList.contains('open');

            closeAllMenus();
            if (!isOpen) openMenu(key);
        });
    });

    // Open menu when section is active (on page load)
    const activeSection = document.querySelector('.workspace-section.active');
    if (activeSection) {
        const sectionId = activeSection.id;
        if (sectionId === 'welcomeSection') {
            // Dashboard is active - highlight Dashboard menu item
            const dashboardItem = document.querySelector('.sidebar-item');
            if (dashboardItem) {
                dashboardItem.classList.add('active');
            }
        } else if (sectionId === 'feeSetup' || sectionId === 'classFee' || sectionId === 'challanForm' || sectionId === 'generatedChallan') {
            openMenu('fee');
        } else if (sectionId === 'classSetup' || sectionId === 'addSection' || sectionId === 'allocateTeacher') {
            openMenu('class');
        } else if (sectionId === 'addStaff' || sectionId === 'addEducation' || sectionId === 'viewStaff') {
            openMenu('staff');
        }
    }
}

// ========== FEE MANAGEMENT ==========
function initializeFeeManagement() {
    // Fee Setup Form
    const feeForm = document.getElementById('feeForm');
    if (feeForm) {
        feeForm.addEventListener('submit', handleFeeFormSubmit);
    }

    // Filter Category
    const filterCategory = document.getElementById('filterCategory');
    if (filterCategory) {
        filterCategory.addEventListener('change', loadFees);
    }
}

let editingFeeId = null;

function resetFeeForm() {
    const form = document.getElementById('feeForm');
    if (!form) return;
    form.reset();
    document.getElementById('submitBtn').textContent = 'Set Feehead';
    document.getElementById('feeId').value = '';
    document.getElementById('cancelEdit').classList.remove('show');
    editingFeeId = null;
}

function editFee(fee) {
    document.getElementById('category').value = fee.category;
    document.getElementById('feeHead').value = fee.feeHead;
    document.getElementById('feeId').value = fee.feeId;
    editingFeeId = fee.feeId;

    document.getElementById('submitBtn').textContent = 'Update Feehead';
    document.getElementById('cancelEdit').classList.add('show');
}

function deleteFee(feeId) {
    if (!confirm('Are you sure you want to delete this fee?')) return;

    fetch(`/Admin/AdminDashboard/DeleteFeeHead/${feeId}`, {
        method: 'DELETE'
    })
        .then(res => {
            if (!res.ok) throw new Error('Delete failed');
            return res.json();
        })
        .then(() => {
            loadFees();
            resetFeeForm();
        })
        .catch(error => {
            console.error('Error deleting fee:', error);
            alert('Error deleting fee: ' + error.message);
        });
}

function handleFeeFormSubmit(e) {
    e.preventDefault();

    const category = document.getElementById('category').value;
    const feeHEAD = document.getElementById('feeHead').value;

    if (!category || !feeHEAD) {
        alert('Please fill all fields');
        return;
    }

    const url = editingFeeId ? `/Admin/AdminDashboard/UpdateFeeHead/${editingFeeId}` : `/Admin/AdminDashboard/CreateFeeHead`;
    const method = editingFeeId ? 'PUT' : 'POST';

    fetch(url, {
        method: method,
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
        },
        body: JSON.stringify({
            Category: category,
            feeHEAD: feeHEAD,
            schoolID: 123
        })
    })
        .then(res => {
            if (!res.ok) {
                return res.text().then(text => { throw new Error(text); });
            }
            return res.json();
        })
        .then(result => {
            alert(result.message || 'Operation successful!');
            loadFees();
            resetFeeForm();
        })
        .catch(error => {
            console.error('Error saving fee:', error);
            alert('Error: ' + error.message);
        });
}

function loadFees() {
    const selectedValue = document.getElementById('filterCategory')?.value;
    const tbody = document.querySelector('#feeTable tbody');

    if (!tbody) return;

    if (!selectedValue) {
        tbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">Please select a category</td></tr>';
        return;
    }

    fetch(`/Admin/AdminDashboard/GetFeeBySchool?category=${encodeURIComponent(selectedValue)}`)
        .then(response => {
            if (!response.ok) throw new Error('Network response was not ok');
            return response.json();
        })
        .then(data => {
            tbody.innerHTML = '';
            if (data && data.length > 0) {
                data.forEach(fee => {
                    const tr = document.createElement('tr');
                    const editBtn = document.createElement('button');
                    editBtn.type = 'button';
                    editBtn.textContent = 'Edit';
                    editBtn.style.marginRight = '5px';
                    editBtn.onclick = () => editFee(fee);

                    const delBtn = document.createElement('button');
                    delBtn.type = 'button';
                    delBtn.textContent = 'Delete';
                    delBtn.onclick = () => deleteFee(fee.feeId);

                    tr.innerHTML = `
                        <td>${escapeHtml(fee.category)}</td>
                        <td>${escapeHtml(fee.feeHead)}</td>
                        <td></td>
                    `;
                    tr.querySelector('td:last-child').appendChild(editBtn);
                    tr.querySelector('td:last-child').appendChild(delBtn);
                    tbody.appendChild(tr);
                });
            } else {
                tbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">No fee heads found</td></tr>';
            }
        })
        .catch(error => {
            console.error('Error fetching fee data:', error);
            tbody.innerHTML = '<tr><td colspan="3" style="text-align: center; color: red;">Error loading data</td></tr>';
        });
}

// ========== CLASS FEE MANAGEMENT ==========
function initializeClassFeeManagement() {
    const classFeeForm = document.getElementById('classFeeForm');
    if (classFeeForm) {
        classFeeForm.addEventListener('submit', handleClassFeeFormSubmit);
    }

    const categorySelect = document.getElementById('classFeeCategory');
    if (categorySelect) {
        categorySelect.addEventListener('change', function () {
            const showClassField = this.value === 'Class Fee Charges';
            const classFieldEl = document.getElementById('classField');
            if (showClassField) {
                classFieldEl.classList.remove('class-field-hidden');
                classFieldEl.classList.add('class-field-visible');
                // Small delay to ensure DOM is updated, then load classes
                setTimeout(() => {
                    console.log('Loading classes after category change to:', this.value);
                    loadClassesForClassFee(this.value);
                }, 50);
            } else {
                classFieldEl.classList.remove('class-field-visible');
                classFieldEl.classList.add('class-field-hidden');
                const classIDEl = document.getElementById('classID');
                if (classIDEl) {
                    classIDEl.value = '';
                }
            }
            loadFeeHeadsForClassFee(this.value);
        });
    } else {
        console.warn('classFeeCategory dropdown not found!');
    }

    const tableCategory = document.getElementById('tablecategory');
    if (tableCategory) {
        tableCategory.addEventListener('change', function () {
            const type = this.value;
            const showTableClassField = type === 'Class Fee Charges';
            const tableClassField = document.getElementById('tableclassField');
            if (showTableClassField) {
                tableClassField.classList.remove('class-field-hidden');
                tableClassField.classList.add('class-field-visible');
                // Small delay to ensure DOM is updated, then load classes
                setTimeout(() => {
                    console.log('Loading classes for table after category change to:', type);
                    loadClassesForClassFee(type);
                }, 50);
            } else {
                tableClassField.classList.remove('class-field-visible');
                tableClassField.classList.add('class-field-hidden');
            }
            const tableClassIDEl = document.getElementById('tableclassID');
            if (tableClassIDEl) {
                tableClassIDEl.value = '';
            }

            if (type && type !== 'Class Fee Charges') {
                loadClassFeeTableData(type, -1);
            } else if (type === 'Class Fee Charges') {
                const tbody = document.querySelector('#classFeeTable tbody');
                if (tbody) {
                    tbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">Please select a class</td></tr>';
                }
            } else {
                const tbody = document.querySelector('#classFeeTable tbody');
                if (tbody) {
                    tbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">Please select a category</td></tr>';
                }
            }
        });
    } else {
        console.warn('tablecategory dropdown not found!');
    }

    const tableClassID = document.getElementById('tableclassID');
    if (tableClassID) {
        tableClassID.addEventListener('change', function () {
            const classID = this.value;
            const category = document.getElementById('tablecategory').value;
            if (classID && category === 'Class Fee Charges') {
                loadClassFeeTableData(category, classID);
            }
        });
    }
}
document.addEventListener('DOMContentLoaded', () => {
    // Left side (form) elements
    const categorySelect = document.getElementById('classFeeCategory');
    const classField = document.getElementById('classField');
    const classDropdown = document.getElementById('classID');
    const feeHeadSelect = document.getElementById('feeHeadS');

    // Right side (table) elements
    const tableCategorySelect = document.getElementById('tablecategory');
    const tableClassField = document.getElementById('tableclassField');
    const tableClassDropdown = document.getElementById('tableclassID');

    // =================== LEFT SIDE ===================

    categorySelect.addEventListener('change', () => {
        const selectedCategory = categorySelect.value;

        if (!selectedCategory) {
            classField.style.display = 'none';
            classDropdown.innerHTML = '<option value="">--Select--</option>';
            feeHeadSelect.innerHTML = '<option value="">--Select--</option>';
            return;
        }

        classField.style.display = 'block';
        loadClasses(selectedCategory, classDropdown);
        loadFeeHeads(selectedCategory);
    });

    // =================== RIGHT SIDE ===================

    tableCategorySelect.addEventListener('change', () => {
        const selectedCategory = tableCategorySelect.value;

        if (!selectedCategory) {
            tableClassField.style.display = 'none';
            tableClassDropdown.innerHTML = '<option value="">--Select--</option>';
            return;
        }

        tableClassField.style.display = 'block';
        loadClasses(selectedCategory, tableClassDropdown);
    });

    // =================== COMMON FUNCTIONS ===================

    // Load classes from server filtered by category
    function loadClasses(category, dropdown) {
        fetch('/Admin/AdminDashboard/GetClasses')
            .then(res => res.json())
            .then(data => {
                console.log('Classes fetched:', data);

                // If your JSON has category field, filter by it
                const filtered = category ? data.filter(c => c.category === category) : data;

                dropdown.innerHTML = '<option value="">--Select--</option>';
                filtered.forEach(c => {
                    const opt = document.createElement('option');
                    opt.value = c.id;
                    opt.textContent = c.description;
                    dropdown.appendChild(opt);
                });

                console.log('Dropdown populated:', dropdown.id);
            })
            .catch(err => {
                console.error('Error loading classes:', err);
                dropdown.innerHTML = '<option disabled>Error loading classes</option>';
            });
    }

    // Load fee heads for left form dropdown
    function loadFeeHeads(category) {
        fetch(`/Admin/AdminDashboard/GetFeeHeads?category=${encodeURIComponent(category)}`)
            .then(res => res.json())
            .then(data => {
                console.log('Fee heads fetched:', data);
                feeHeadSelect.innerHTML = '<option value="">--Select--</option>';

                if (!data || data.length === 0) {
                    feeHeadSelect.innerHTML = '<option disabled>No fee heads available</option>';
                    return;
                }

                data.forEach(f => {
                    const opt = document.createElement('option');
                    opt.value = f.id;
                    opt.textContent = f.feeHEAD;
                    feeHeadSelect.appendChild(opt);
                });
            })
            .catch(err => {
                console.error('Error loading fee heads:', err);
                feeHeadSelect.innerHTML = '<option disabled>Error loading fee heads</option>';
            });
    }
});

function loadClassFeeTableData(category, classID) {
    const tbody = document.querySelector('#classFeeTable tbody');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">Loading...</td></tr>';

    const url = classID && classID !== -1
        ? `/Admin/AdminDashboard/GetFeeByClass?classID=${encodeURIComponent(classID)}&type=class`
        : `/Admin/AdminDashboard/GetFeeByClass?classID=-1&type=${encodeURIComponent(category)}`;

    fetch(url)
        .then(res => {
            if (!res.ok) throw new Error('Failed to fetch fee data');
            return res.json();
        })
        .then(data => {
            tbody.innerHTML = '';
            if (data && data.length > 0) {
                data.forEach(fee => {
                    const tr = document.createElement('tr');
                    tr.dataset.facId = fee.facId;
                    tr.dataset.fcmId = fee.fcmId || '';

                    const actionTd = fee.fcmId && fee.fcmId > 0
                        ? `<button style="margin-right: 5px;" onclick="editClassFee(${fee.fcmId})">Edit</button>
                           <button onclick="deleteClassFee(${fee.fcmId})">Delete</button>`
                        : `<button onclick="createFeeFromTable(${fee.facId}, ${document.getElementById('tableclassID').value || 'null'})">Create</button>`;

                    tr.innerHTML = `
                        <td>${escapeHtml(fee.feeHead)}</td>
                        <td>${escapeHtml(fee.feeAmount)}</td>
                        <td>${actionTd}</td>
                    `;
                    tbody.appendChild(tr);
                });
            } else {
                tbody.innerHTML = '<tr><td colspan="3" style="text-align: center;">No fee records found</td></tr>';
            }
        })
        .catch(err => {
            console.error('Error loading table data:', err);
            tbody.innerHTML = '<tr><td colspan="3" style="text-align: center; color: red;">Error loading data</td></tr>';
        });
}

function resetClassFeeForm() {
    const form = document.getElementById('classFeeForm');
    if (!form) return;
    form.reset();
    document.getElementById('classField').classList.add('class-field-hidden');
    document.getElementById('classField').classList.remove('class-field-visible');
    document.getElementById('submitClassFeeBtn').textContent = 'Set Fee';
    document.getElementById('editId').value = '';
    document.getElementById('cancelClassFeeEdit').classList.remove('show');
}

function handleClassFeeFormSubmit(e) {
    e.preventDefault();

    const form = e.target;
    const formData = new FormData(form);

    // Get anti-forgery token if available
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }

    // Show loading state
    const submitBtn = document.getElementById('submitClassFeeBtn');
    const originalText = submitBtn.textContent;
    submitBtn.disabled = true;
    submitBtn.textContent = 'Saving...';

    fetch(form.action, {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            // Check if response is JSON or HTML (redirect/view)
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return response.json();
            } else {
                // If it's HTML (view/redirect), treat as success for now
                return { success: true, message: 'Fee saved successfully!' };
            }
        })
        .then(data => {
            if (data.success) {
                alert(data.message || 'Fee saved successfully!');
                resetClassFeeForm();

                // Reload the table data
                const category = document.getElementById('tablecategory').value;
                const classID = document.getElementById('tableclassID')?.value || -1;
                if (category) {
                    if (category === 'Class Fee Charges' && classID && classID !== '-1') {
                        loadClassFeeTableData(category, classID);
                    } else if (category !== 'Class Fee Charges') {
                        loadClassFeeTableData(category, -1);
                    }
                }
            } else {
                // Show validation errors
                let errorMsg = data.message || 'Error saving fee';
                if (data.errors && Array.isArray(data.errors) && data.errors.length > 0) {
                    const errorDetails = data.errors.map(e => {
                        if (e && e.Field && e.Errors && Array.isArray(e.Errors)) {
                            return `${e.Field}: ${e.Errors.join(', ')}`;
                        } else if (e && typeof e === 'string') {
                            return e;
                        } else if (e && e.Field) {
                            return `${e.Field}: ${JSON.stringify(e.Errors || e)}`;
                        }
                        return JSON.stringify(e);
                    }).filter(msg => msg).join('\n');
                    if (errorDetails) {
                        errorMsg += '\n' + errorDetails;
                    }
                }
                alert(errorMsg);
            }
        })
        .catch(error => {
            console.error('Error saving fee:', error);
            alert('Error saving fee: ' + error.message);
        })
        .finally(() => {
            submitBtn.disabled = false;
            submitBtn.textContent = originalText;
        });
}

function editClassFee(id) {
    console.log('editClassFee called with id:', id);

    if (!id || id <= 0) {
        alert('Invalid fee ID');
        return;
    }

    fetch(`/Admin/AdminDashboard/GetFeeDetails?id=${id}`)
        .then(res => {
            console.log('GetFeeDetails response status:', res.status);
            if (!res.ok) {
                return res.text().then(text => {
                    console.error('Response error:', text);
                    throw new Error(`HTTP ${res.status}: ${text}`);
                });
            }
            return res.json();
        })
        .then(fee => {
            console.log('Fee details received:', fee);

            if (!fee) {
                throw new Error('No fee data returned from server');
            }

            if (fee.success === false) {
                throw new Error(fee.message || 'Fee not found');
            }

            const editIdEl = document.getElementById('editId');
            const editIdHiddenEl = document.getElementById('editIdHidden');
            if (editIdEl) editIdEl.value = fee.id && fee.id > 0 ? fee.id : '';
            if (editIdHiddenEl) editIdHiddenEl.value = fee.id && fee.id > 0 ? fee.id : '';

            const categoryEl = document.getElementById('classFeeCategory');
            if (categoryEl) {
                categoryEl.value = fee.category || '';
            }

            const showClassField = fee.category === 'Class Fee Charges';
            const classFieldEl = document.getElementById('classField');
            if (classFieldEl) {
                if (showClassField) {
                    classFieldEl.classList.remove('class-field-hidden');
                    classFieldEl.classList.add('class-field-visible');
                } else {
                    classFieldEl.classList.remove('class-field-visible');
                    classFieldEl.classList.add('class-field-hidden');
                }
            }

            if (fee.category) {
                // Load classes if needed
                if (fee.category === 'Class Fee Charges') {
                    loadClassesForClassFee(fee.category).then(() => {
                        if (fee.classID) {
                            setTimeout(() => {
                                const classIDEl = document.getElementById('classID');
                                if (classIDEl) {
                                    classIDEl.value = fee.classID;
                                }
                            }, 300);
                        }
                    }).catch(err => {
                        console.error('Error loading classes:', err);
                    });
                }

                // Load fee heads and set the selected value
                loadFeeHeadsForClassFee(fee.category).then(() => {
                    const feeHeadSelect = document.getElementById('feeHeadS');
                    if (feeHeadSelect && fee.feeHead) {
                        feeHeadSelect.value = fee.feeHead;
                    }
                }).catch(err => {
                    console.error('Error loading fee heads:', err);
                });
            }

            const amountEl = document.getElementById('amount');
            if (amountEl) {
                amountEl.value = fee.feeAmount || 0;
            }

            const submitBtn = document.getElementById('submitClassFeeBtn');
            if (submitBtn) {
                submitBtn.textContent = (fee.id && fee.id > 0) ? 'Update Fee' : 'Create Fee';
            }

            const cancelBtn = document.getElementById('cancelClassFeeEdit');
            if (cancelBtn) {
                cancelBtn.style.display = 'inline-block';
            }

            window.scrollTo({ top: 0, behavior: 'smooth' });
        })
        .catch(err => {
            console.error('Error fetching fee details:', err);
            alert('Error loading fee details: ' + err.message);
        });
}

function deleteClassFee(id) {
    if (!confirm('Are you sure you want to delete this fee?')) return;

    fetch(`/Admin/AdminDashboard/DeleteFee?id=${id}`, {
        method: 'DELETE'
    })
        .then(res => {
            if (!res.ok) throw new Error('Delete failed');
            return res.json();
        })
        .then(result => {
            if (result.success) {
                const category = document.getElementById('tablecategory').value;
                const classID = document.getElementById('tableclassID').value;
                loadClassFeeTableData(category, classID || -1);
                resetClassFeeForm();
            } else {
                throw new Error(result.message || 'Delete failed');
            }
        })
        .catch(err => {
            console.error('Delete failed', err);
            alert('Error deleting fee: ' + err.message);
        });
}

function createFeeFromTable(facId, classId) {
    console.log('createFeeFromTable called with facId:', facId, 'classId:', classId);

    if (!facId || facId <= 0) {
        alert('Invalid fee head ID');
        return;
    }

    // GetFeeDetails can handle both fcm_id (existing record) and fac_id (fee head only)
    fetch(`/Admin/AdminDashboard/GetFeeDetails?id=${facId}`)
        .then(res => {
            console.log('GetFeeDetails response status:', res.status);
            if (!res.ok) {
                return res.text().then(text => {
                    console.error('Response error:', text);
                    throw new Error(`HTTP ${res.status}: ${text}`);
                });
            }
            return res.json();
        })
        .then(fee => {
            console.log('Fee head details received:', fee);

            if (!fee) {
                throw new Error('No fee head data returned from server');
            }

            if (fee.success === false) {
                throw new Error(fee.message || 'Fee head not found');
            }

            resetClassFeeForm();

            const category = fee.category || 'Class Fee Charges';
            const categoryEl = document.getElementById('classFeeCategory');
            if (categoryEl) {
                categoryEl.value = category;
            }

            const classFieldEl = document.getElementById('classField');
            if (category === 'Class Fee Charges') {
                if (classFieldEl) {
                    classFieldEl.classList.remove('class-field-hidden');
                    classFieldEl.classList.add('class-field-visible');
                }

                // Load classes first
                loadClassesForClassFee(category).then(() => {
                    if (classId && classId !== 'null' && classId !== null && classId !== 'undefined' && classId !== '') {
                        setTimeout(() => {
                            const classIDEl = document.getElementById('classID');
                            if (classIDEl) {
                                classIDEl.value = classId;
                            }
                        }, 300);
                    }
                }).catch(err => {
                    console.error('Error loading classes:', err);
                });
            } else {
                if (classFieldEl) {
                    classFieldEl.classList.remove('class-field-visible');
                    classFieldEl.classList.add('class-field-hidden');
                }
            }

            // Load fee heads and set the selected value
            loadFeeHeadsForClassFee(category).then(() => {
                const feeHeadSelect = document.getElementById('feeHeadS');
                if (feeHeadSelect && fee.feeHead) {
                    feeHeadSelect.value = fee.feeHead;
                }
            }).catch(err => {
                console.error('Error loading fee heads:', err);
            });

            // Clear edit ID since we're creating new
            const editIdEl = document.getElementById('editId');
            const editIdHiddenEl = document.getElementById('editIdHidden');
            if (editIdEl) editIdEl.value = '';
            if (editIdHiddenEl) editIdHiddenEl.value = '';

            const cancelBtn = document.getElementById('cancelClassFeeEdit');
            if (cancelBtn) {
                cancelBtn.style.display = 'inline-block';
            }

            window.scrollTo({ top: 0, behavior: 'smooth' });
        })
        .catch(err => {
            console.error('Error creating fee from table:', err);
            alert('Error setting up form: ' + err.message);
        });
}

// ========== CLASS MANAGEMENT ==========
function initializeClassManagement() {
    const classForm = document.getElementById('classForm');

    if (classForm) {
        // Validate prefix input (uppercase, only letters, max 3)
        const prefixInput = document.getElementById('classPrefix');
        if (prefixInput) {
            prefixInput.addEventListener('input', function () {
                this.value = this.value.toUpperCase().replace(/[^A-Z]/g, '').slice(0, 3);
            });
        }

        // Handle form submission
        classForm.addEventListener('submit', async function (event) {
            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();

            // Prevent double submission
            if (isClassFormSubmitting) {
                console.warn('Class form submission already in progress');
                return;
            }
            isClassFormSubmitting = true;

            // Basic validation
            const className = document.getElementById('className').value.trim();
            const classPrefix = document.getElementById('classPrefix').value.trim();
            const category = document.getElementById('classDescription').value;
            const subjectWiseTeaching = document.getElementById('subjectWiseTeaching').value;

            if (!className || classPrefix.length !== 3 || !category || !subjectWiseTeaching) {
                alert('Please fill all fields correctly. Prefix must be exactly 3 letters.');
                isClassFormSubmitting = false;
                return;
            }

            // Disable button and show loading
            const submitButton = this.querySelector('button[type="submit"]');
            const originalText = submitButton.textContent;
            submitButton.textContent = 'Adding...';
            submitButton.disabled = true;

            try {
                const data = {
                    ClassSetup: {
                        ClassSetupModel: {
                            className: className,
                            prefix: classPrefix,
                            category: category,
                            subjectWiseTeaching: subjectWiseTeaching
                        }
                    }
                };

                const response = await fetch('/Admin/AdminDashboard/ClassSetup', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify(data)
                });

                if (!response.ok) {
                    const text = await response.text();
                    throw new Error(`HTTP ${response.status}: ${text || 'Server error'}`);
                }

                const result = await response.json();

                if (result.success) {
                    alert(result.message);
                    this.reset();
                } else {
                    alert(result.message || 'Failed to add class');
                }
            } catch (error) {
                console.error('Error:', error);
                alert('Error: ' + error.message);
            } finally {
                // Restore button
                submitButton.textContent = originalText;
                submitButton.disabled = false;
                isClassFormSubmitting = false;
            }
        });
    }
}
function initializeSectionManagement() {
    const sectionForm = document.getElementById('sectionForm');

    if (sectionForm) {
        // Validate section prefix input (uppercase, only letters, max 3)
        const prefixInput = document.getElementById('sectionPrefix');
        if (prefixInput) {
            prefixInput.addEventListener('input', function () {
                this.value = this.value.toUpperCase().replace(/[^A-Z]/g, '').slice(0, 3);
            });
        }

        // Validate year input (only numbers, max 4 digits)
        const yearInput = document.getElementById('sectionYear');
        if (yearInput) {
            yearInput.addEventListener('input', function () {
                this.value = this.value.replace(/[^0-9]/g, '').slice(0, 4);
            });

            yearInput.addEventListener('blur', function () {
                if (this.value.length !== 4 && this.value.length > 0) {
                    alert('Year must be exactly 4 digits');
                    this.focus();
                }
            });
        }

        // Handle form submission
        sectionForm.addEventListener('submit', async function (event) {
            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();

            // Prevent double submission
            if (isSectionFormSubmitting) {
                console.warn('Section form submission already in progress');
                return;
            }
            isSectionFormSubmitting = true;

            // Get form values
            const classId = document.getElementById('sectionClassDropdown').value;
            const prefix = document.getElementById('sectionPrefix').value.trim();
            const description = document.getElementById('sectionDescription').value.trim();
            const year = document.getElementById('sectionYear').value.trim();
            const requiresAssistant = document.getElementById('requiresAssistant').value;

            // Validation
            if (!classId) {
                alert('Please select a class');
                isSectionFormSubmitting = false;
                return;
            }

            if (prefix.length !== 3) {
                alert('Section prefix must be exactly 3 letters');
                isSectionFormSubmitting = false;
                return;
            }

            if (!description) {
                alert('Please enter section description');
                isSectionFormSubmitting = false;
                return;
            }

            if (year.length !== 4) {
                alert('Year must be exactly 4 digits');
                isSectionFormSubmitting = false;
                return;
            }

            if (!requiresAssistant) {
                alert('Please select if assistant is required');
                isSectionFormSubmitting = false;
                return;
            }

            // Disable button and show loading
            const submitButton = this.querySelector('button[type="submit"]');
            const originalText = submitButton.textContent;
            submitButton.textContent = 'Adding Section...';
            submitButton.disabled = true;

            try {
                const data = {
                    ClassSetup: {
                        classSectionModel: {
                            classId: parseInt(classId),
                            prefix: prefix,
                            description: description,
                            year: year,
                            requiredAssistant: requiresAssistant
                        }
                    }
                };

                const response = await fetch('/Admin/AdminDashboard/AddSection', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify(data)
                });

                if (!response.ok) {
                    const text = await response.text();
                    throw new Error(`HTTP ${response.status}: ${text || 'Server error'}`);
                }

                const result = await response.json();

                if (result.success) {
                    alert(result.message);
                    this.reset();
                    // Reset dropdown to default
                    document.getElementById('sectionClassDropdown').value = '';
                } else {
                    alert(result.message || 'Failed to add section');
                }
            } catch (error) {
                console.error('Error:', error);
                alert('Error: ' + error.message);
            } finally {
                // Restore button
                submitButton.textContent = originalText;
                submitButton.disabled = false;
                isSectionFormSubmitting = false;
            }
        });
    }
}
function loadClassesForSection() {
    const dropdown = document.getElementById('sectionClassDropdown');
    if (!dropdown) return;

    dropdown.innerHTML = '<option value="">Loading...</option>';

    fetch('/Admin/AdminDashboard/GetClassesSetup')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            dropdown.innerHTML = '<option value="">-- Select Class --</option>';
            if (data && Array.isArray(data)) {
                data.forEach(c => {
                    const opt = document.createElement('option');
                    opt.value = c.Id || c.id;
                    opt.textContent = c.Description || c.description;
                    dropdown.appendChild(opt);
                });
            }
        })
        .catch(error => {
            console.error('Error loading classes:', error);
            dropdown.innerHTML = '<option value="">Error loading classes</option>';
        });
}
function setupSubjectForm() {
    const subjectForm = document.getElementById('subjectForm');
    if (!subjectForm) return;

    // Remove any existing listeners by cloning
    const newForm = subjectForm.cloneNode(true);
    subjectForm.parentNode.replaceChild(newForm, subjectForm);

    const form = document.getElementById('subjectForm');
    if (!form) return;

    form.addEventListener('submit', async function (event) {
        event.preventDefault();
        event.stopPropagation();
        event.stopImmediatePropagation();

        // Prevent double submission
        if (isSubjectFormSubmitting) {
            console.warn('Subject form submission already in progress');
            return;
        }
        isSubjectFormSubmitting = true;

        // Get form values directly from inputs
        const classID = document.getElementById('subjectClassDropdown')?.value;
        const subjectID = document.getElementById('subjectDropdown')?.value;
        const year = document.getElementById('subjectClassyear')?.value;

        if (!classID || !subjectID || !year) {
            alert('Please fill all fields');
            isSubjectFormSubmitting = false;
            return;
        }

        const yearNum = parseInt(year);
        if (isNaN(yearNum) || yearNum <= 0) {
            alert('Please enter a valid year (e.g., 2025)');
            isSubjectFormSubmitting = false;
            return;
        }

        const classIdNum = parseInt(classID);
        const subjectIdNum = parseInt(subjectID);
        if (isNaN(classIdNum) || isNaN(subjectIdNum) || classIdNum <= 0 || subjectIdNum <= 0) {
            alert('Please select valid class and subject');
            isSubjectFormSubmitting = false;
            return;
        }

        // Create an object from form data
        const data = {
            ClassSetup: {
                AddSubject: {
                    classID: classIdNum,
                    subjectID: subjectIdNum,
                    year: yearNum
                }
            }
        };

        // Show loading state
        const submitButton = form.querySelector('button[type="submit"]');
        const originalButtonText = submitButton.textContent;
        submitButton.textContent = editingSubjectId ? 'Updating...' : 'Adding...';
        submitButton.disabled = true;

        try {
            const url = editingSubjectId
                ? `/Admin/AdminDashboard/UpdateSubject/${editingSubjectId}`
                : '/Admin/AdminDashboard/AddSubject';
            const method = editingSubjectId ? 'PUT' : 'POST';

            const response = await fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                const text = await response.text();
                throw new Error(`HTTP ${response.status}: ${text || 'Server error'}`);
            }

            const contentType = response.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                const text = await response.text();
                throw new Error(`Invalid response format: ${text}`);
            }

            const result = await response.json();

            if (result.success) {
                // Show success message
                alert(result.message || (editingSubjectId ? 'Subject updated successfully!' : 'Subject added successfully!'));

                // Reset form
                form.reset();
                editingSubjectId = null;
                submitButton.textContent = 'Add Subject';

                // Refresh the subjects table
                loadSubjectsTable();
            } else {
                // Show error message
                alert(result.message || 'Failed to add subject.');
            }
        } catch (error) {
            console.error('Error saving subject:', error);
            alert('Error: ' + error.message);
        } finally {
            // Reset button state
            submitButton.textContent = originalButtonText;
            submitButton.disabled = false;
            isSubjectFormSubmitting = false;
        }
    });
}
function initClassTimeTable() {

    const classDDL = document.getElementById('TimeTableClass');
    const sectionDDL = document.getElementById('TimeTableSection');
    const btnGenerate = document.getElementById('btnGenerateTimeTable');
    const container = document.getElementById('timeTableContainer');

    // ---------- LOAD CLASSES ----------
    function loadClasses() {
        fetch('/Admin/AdminDashboard/GetClasses')
            .then(res => res.json())
            .then(data => {
                classDDL.innerHTML = `<option value="">--Select Class--</option>`;
                data.forEach(c => {
                    classDDL.innerHTML +=
                        `<option value="${c.id}">${c.description}</option>`;
                });
            });
    }

    // ---------- LOAD SECTIONS ----------
    function loadSections(classId) {
        fetch(`/Admin/AdminDashboard/GetSectionByClass/${classId}`)
            .then(res => res.json())
            .then(data => {
                sectionDDL.innerHTML = `<option value="">--Select Section--</option>`;
                btnGenerate.style.display = 'none';
                data.forEach(s =>
                    sectionDDL.innerHTML += `<option value="${s.id}">${s.prefix}</option>`
                );
            });
    }

    // ---------- LOAD TIMETABLE ----------
    function loadTimeTable(classId, sectionId) {

        fetch(`/Admin/AdminDashboard/GetTimeTable?classId=${classId}&sectionId=${sectionId}`)
            .then(res => res.json())
            .then(data => {

                let html = `
                <table border="1">
                    <tr>
                        <th>Time</th>
                        <th>Mon</th>
                        <th>Tue</th>
                        <th>Wed</th>
                        <th>Thu</th>
                        <th>Fri</th>
                        <th>Sat</th>
                    </tr>`;

                data.forEach(r => {
                    html += `
                    <tr>
                        <td>${r.timeSlot}</td>
                        <td>${r.monday}</td>
                        <td>${r.tuesday}</td>
                        <td>${r.wednesday}</td>
                        <td>${r.thursday}</td>
                        <td>${r.friday}</td>
                        <td>${r.saturday}</td>
                    </tr>`;
                });

                html += `</table>`;
                container.innerHTML = html;
            });
    }

    // ---------- EVENTS ----------
    classDDL.addEventListener('change', () => {
        const classId = classDDL.value;
        sectionDDL.innerHTML = `<option value="">--Select Section--</option>`;
        btnGenerate.style.display = 'none';
        container.innerHTML = '';

        if (classId) loadSections(classId);
    });

    sectionDDL.addEventListener('change', () => {
        btnGenerate.style.display = sectionDDL.value ? 'inline-block' : 'none';
    });

    btnGenerate.addEventListener('click', () => {
        loadTimeTable(classDDL.value, sectionDDL.value);
    });

    // ---------- INIT ----------
    loadClasses();
}
function initializeSubject() {
    setupSubjectForm();
    loadSubjects();
    loadClassesforSubjects();
    loadSubjectsTable();
}
function loadSubjectsTable() {
    const tbody = document.querySelector('#addSubjectView tbody');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="4">Loading...</td></tr>';

    fetch('/Admin/AdminDashboard/GetClassSubjects')
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            return response.json();
        })
        .then(data => {
            tbody.innerHTML = '';
            if (!data || data.length === 0) {
                tbody.innerHTML = '<tr><td colspan="4" style="text-align: center;">No subjects added yet.</td></tr>';
                return;
            }

            data.forEach(subject => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${escapeHtml(subject.subjectName || 'N/A')}</td>
                    <td>${escapeHtml(subject.className || 'N/A')}</td>
                    <td>${escapeHtml(subject.year || 'N/A')}</td>
                    <td>
                        <button class="btn-edit" onclick="editSubject(${subject.id})" style="margin-right: 5px; padding: 5px 10px; background: #4caf50; color: white; border: none; border-radius: 3px; cursor: pointer;">Edit</button>
                        <button class="btn-delete" onclick="deleteSubject(${subject.id})" style="padding: 5px 10px; background: #f44336; color: white; border: none; border-radius: 3px; cursor: pointer;">Delete</button>
                    </td>
                `;
                tbody.appendChild(row);
            });
        })
        .catch(error => {
            console.error('Error loading subjects table:', error);
            tbody.innerHTML = '<tr><td colspan="4" style="text-align: center; color: red;">Error loading subjects: ' + error.message + '</td></tr>';
        });
}
function editSubject(subjectId) {
    if (!subjectId || subjectId <= 0) {
        alert('Invalid subject ID: ' + subjectId);
        return;
    }

    console.log('Editing subject ID:', subjectId);

    fetch(`/Admin/AdminDashboard/GetSubjectById/${subjectId}`)
        .then(res => {
            if (!res.ok) {
                return res.text().then(text => {
                    throw new Error(`HTTP ${res.status}: ${text || 'Subject not found'}`);
                });
            }
            const contentType = res.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                return res.text().then(text => {
                    throw new Error(`Invalid response format: ${text}`);
                });
            }
            return res.json();
        })
        .then(result => {
            console.log('Edit result:', result);
            if (!result.success || !result.data) {
                throw new Error(result.message || 'Subject not found');
            }

            const subject = result.data;
            editingSubjectId = subject.id;

            const classDropdown = document.getElementById('subjectClassDropdown');
            const subjectDropdown = document.getElementById('subjectDropdown');
            const yearInput = document.getElementById('subjectClassyear');

            if (classDropdown) {
                loadClassesforSubjects().then(() => {
                    setTimeout(() => {
                        classDropdown.value = subject.classID;
                    }, 100);
                });
            }

            if (subjectDropdown) {
                loadSubjects().then(() => {
                    setTimeout(() => {
                        subjectDropdown.value = subject.subjectID;
                    }, 100);
                });
            }

            if (yearInput) {
                yearInput.value = subject.year || '';
            }

            const submitButton = document.querySelector('#subjectForm button[type="submit"]');
            if (submitButton) {
                submitButton.textContent = 'Update Subject';
            }

            window.scrollTo({ top: 0, behavior: 'smooth' });
        })
        .catch(err => {
            console.error('Error fetching subject details:', err);
            alert('Error loading subject details: ' + err.message);
        });
}


// Example delete function
function deleteSubject(subjectId) {
    if (!subjectId || subjectId <= 0) {
        alert('Invalid subject ID: ' + subjectId);
        return;
    }

    if (!confirm('Are you sure you want to delete this subject?')) return;

    fetch(`/Admin/AdminDashboard/DeleteSubject/${subjectId}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(`HTTP ${response.status}: ${text || 'Delete failed'}`);
                });
            }
            const contentType = response.headers.get("content-type");
            if (!contentType || !contentType.includes("application/json")) {
                return response.text().then(text => {
                    throw new Error(`Invalid response format: ${text}`);
                });
            }
            return response.json();
        })
        .then(result => {
            if (result.success) {
                alert(result.message || 'Subject deleted successfully!');
                loadSubjectsTable();
            } else {
                alert(result.message || 'Failed to delete subject.');
            }
        })
        .catch(error => {
            console.error('Error deleting subject:', error);
            alert('An error occurred while deleting the subject: ' + error.message);
        });
}
function loadClassesforSubjects() {
    const dropdown = document.getElementById('subjectClassDropdown');
    if (!dropdown) return;

    dropdown.innerHTML = '<option value="">Loading...</option>';

    fetch('/Admin/AdminDashboard/GetClassesSetup')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            dropdown.innerHTML = '<option value="">-- Select Class --</option>';
            if (data && Array.isArray(data)) {
                data.forEach(c => {
                    const opt = document.createElement('option');
                    opt.value = c.Id || c.id;
                    opt.textContent = c.Description || c.description;
                    dropdown.appendChild(opt);
                });
            }
        })
        .catch(error => {
            console.error('Error loading classes:', error);
            dropdown.innerHTML = '<option value="">Error loading</option>';
        });
}
function loadSubjects() {
    const dropdown = document.getElementById('subjectDropdown');
    if (!dropdown) return;

    dropdown.innerHTML = '<option value="">Loading...</option>';

    fetch('/Admin/AdminDashboard/GetSubjects')
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            dropdown.innerHTML = '<option value="">-- Select Subject --</option>';
            data.forEach(subject => {
                dropdown.innerHTML += `<option value="${subject.id}">${subject.name}</option>`;
            });
        })
        .catch(error => {
            console.error('Error loading subjects:', error);
            dropdown.innerHTML = '<option value="">Error loading subjects</option>';
        });
}
function loadClassesForSectionTeachers() {

    const classDropdown = document.getElementById('allocateClass');
    const sectionDropdown = document.getElementById('allocateSection');

    const subjectDiv = document.getElementById('subjectDiv');
    const teacherDiv = document.getElementById('teacherDiv');
    const assistantDiv = document.getElementById('assistantDiv');

    const subjectSelect = document.getElementById('subjectSelect');
    const teacherSelect = document.getElementById('teacherSelect');

    if (!classDropdown) return;

    /* ================= LOAD CLASSES ================= */
    classDropdown.innerHTML = '<option value="">Loading...</option>';

    fetch('/Admin/AdminDashboard/GetClassesSetup')
        .then(res => res.json())
        .then(data => {
            classDropdown.innerHTML = '<option value="">-- Select Class --</option>';
            data.forEach(c => {
                classDropdown.innerHTML +=
                    `<option value="${c.id}">${c.description}</option>`;
            });
        })
        .catch(err => console.error('Class load error:', err));


    /* ================= ON CLASS CHANGE ================= */
    classDropdown.addEventListener('change', function () {

        const classId = this.value;

        /* ---------- RESET UI ---------- */
        sectionDropdown.innerHTML = '<option value="">--Select Section--</option>';
        subjectSelect.innerHTML = '<option value="">--Select Subject--</option>';
        teacherSelect.innerHTML = '<option value="">--Select Teacher--</option>';

        subjectDiv.style.display = 'none';
        teacherDiv.style.display = 'none';
        assistantDiv.style.display = 'none';

        if (!classId) return;


        /* ================= LOAD SECTIONS ================= */
        fetch(`/Admin/AdminDashboard/GetSectionByClass/${classId}`)
            .then(res => res.json())
            .then(data => {
                // Clear previous options first
                sectionDropdown.innerHTML = '';

                data.forEach(section => {
                    const opt = document.createElement('option');
                    opt.value = section.id;
                    opt.textContent = section.prefix;
                    sectionDropdown.appendChild(opt);
                });
            })
            .catch(err => console.error('Section load error:', err));



        /* ================= CHECK TEACHING MODE ================= */
        fetch(`/Admin/AdminDashboard/GetClassTeachingMode/${classId}`)
            .then(res => res.json())
            .then(data => {

                /* ---------- SUBJECT-WISE ---------- */
                if (data.subjectWiseTeaching) {

                    subjectDiv.style.display = 'block';
                    teacherDiv.style.display = 'block';
                    assistantDiv.style.display = 'none';

                    loadSubjectsByClass(classId);
                    loadTeachers();

                }
                /* ---------- TEACHER + ASSISTANT ---------- */
                else {

                    subjectDiv.style.display = 'none';
                    teacherDiv.style.display = 'block';
                    assistantDiv.style.display = 'block';

                    loadTeachers();
                }
            })
            .catch(err => console.error('Teaching mode error:', err));

    });
    document.getElementById('allocateForm').addEventListener('submit', function (e) {
        e.preventDefault();

        const data = {
            classId: document.getElementById('allocateClass').value,
            sectionId: document.getElementById('allocateSection').value,
            subjectId: document.getElementById('subjectSelect').value,
            teacherId: document.getElementById('teacherSelect').value
        };

        fetch('/Admin/AdminDashboard/AllocateTeacher', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
            .then(res => {
                if (!res.ok) throw new Error("Allocation failed");
                return res.text();
            })
            .then(msg => {
                alert(msg);
                this.reset();
            })
            .catch(err => alert(err.message));
    });


    /* ================= LOAD TEACHERS ================= */
    function loadTeachers() {

        teacherSelect.innerHTML = '<option value="">Loading...</option>';

        fetch('/Admin/AdminDashboard/GetTeachers')
            .then(res => res.json())
            .then(data => {
                teacherSelect.innerHTML = '<option value="">--Select Teacher--</option>';
                data.forEach(t => {
                    teacherSelect.innerHTML +=
                        `<option value="${t.id}">${t.name}</option>`;
                });
            })
            .catch(err => console.error('Teacher load error:', err));
    }


    /* ================= LOAD SUBJECTS BY CLASS ================= */
    function loadSubjectsByClass(classId) {

        subjectSelect.innerHTML = '<option value="">Loading...</option>';

        fetch(`/Admin/AdminDashboard/GetSubjectsByClass?classId=${classId}`)
            .then(res => res.json())
            .then(data => {
                subjectSelect.innerHTML = '<option value="">--Select Subject--</option>';
                data.forEach(s => {
                    subjectSelect.innerHTML +=
                        `<option value="${s.id}">${s.name}</option>`;
                });
            })
            .catch(err => console.error('Subject load error:', err));
    }
}

function findTeacher() {
    const id = document.getElementById('teacherIdInput')?.value.trim();

    if (!id) {
        alert('Please enter Teacher ID');
        return;
    }

    fetch(`/Admin/ClassSetup/GetByStaffNumber/${id}`)
        .then(res => {
            if (!res.ok) throw new Error('Teacher not found');
            return res.json();
        })
        .then(t => {
            document.getElementById('teacherDetailsBody').innerHTML = `
                <tr>
                    <td>${escapeHtml(t.name)}</td>
                    <td>${escapeHtml(t.staffNumber)}</td>
                    <td>${escapeHtml(t.contact)}</td>
                    <td>${escapeHtml(t.email)}</td>
                </tr>
            `;
            document.getElementById('teacherDetailsTable').classList.add('show');
            document.getElementById('selectedTeacherId').value = t.stf_id;
        })
        .catch(() => {
            alert('No teacher found for given ID');
            document.getElementById('teacherDetailsTable').classList.remove('show');
        });
}

// ========== STAFF MANAGEMENT ==========
function initializeStaffManagement() {
    const staffForm = document.getElementById('staffForm');
    const btnSaveAndContinue = document.getElementById('btnSaveAndContinue');
    const educationForm = document.getElementById('educationForm');
    const btnAddEducation = document.getElementById('btnAddEducationStaff');
    const btnFinishEducation = document.getElementById('btnFinishEducation');
    const educationTable = document.getElementById('educationTable');

    if (educationTable && !educationTable.querySelector('tbody')) {
        educationTable.innerHTML = `
            <thead>
                <tr>
                    <th>Degree</th>
                    <th>Year</th>
                    <th>Institute</th>
                    <th>Grades</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody></tbody>
        `;
    }

    if (btnSaveAndContinue) {
        btnSaveAndContinue.addEventListener('click', async () => {
            try {
                const formData = new FormData(staffForm);
                const actionUrl = staffForm.getAttribute('action') || '/Admin/AdminDashboard/StaffSetup';

                console.log('Saving staff data...');

                const resp = await fetch(actionUrl, {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                let json = null;
                try {
                    json = await resp.json();
                } catch (e) {
                    console.error('Failed to parse JSON response:', e);
                    throw new Error('Server returned invalid response');
                }

                console.log('Server response:', json);

                if (resp.ok && json && json.success) {
                    if (json.stf_id) {
                        // Set the staff ID in the hidden field
                        document.getElementById('sed_stfid').value = json.stf_id;
                        // Set the staff name in the readonly field
                        document.getElementById('educationSelectedStaff').value = json.name || 'Staff Saved';
                        console.log('Staff saved with ID:', json.stf_id, 'Name:', json.name);
                    }
                    alert(json.message || 'Staff saved successfully!');
                    switchWorkspace('addEducation');
                } else {
                    const msg = (json && json.message) ? json.message : 'Failed to save staff. Check server logs.';
                    alert(msg);
                    console.error('Staff save failed:', json || resp.statusText);
                }
            } catch (err) {
                console.error('Error saving staff:', err);
                alert('Error saving staff: ' + err.message);
            }
        });
    }

    if (btnAddEducation) {
        btnAddEducation.addEventListener('click', (e) => {
            e.preventDefault();

            const degreeName = document.getElementById('sed_degreename').value.trim();
            const yearPassing = document.getElementById('sed_yearpassing').value.trim();
            const instituteName = document.getElementById('sed_institutename').value.trim();
            const grades = document.getElementById('sed_grades').value.trim();

            if (!degreeName) {
                alert('Degree name is required.');
                return;
            }

            const tbody = educationTable.querySelector('tbody');
            const newRow = document.createElement('tr');
            newRow.innerHTML = `
                <td>${escapeHtml(degreeName)}</td>
                <td>${escapeHtml(yearPassing)}</td>
                <td>${escapeHtml(instituteName)}</td>
                <td>${escapeHtml(grades)}</td>
                <td><button type="button" class="deleteBtn" onclick="this.closest('tr').remove()">❌</button></td>
            `;
            tbody.appendChild(newRow);

            document.getElementById('sed_degreename').value = '';
            document.getElementById('sed_yearpassing').value = '';
            document.getElementById('sed_institutename').value = '';
            document.getElementById('sed_grades').value = '';
            document.getElementById('sed_degreename').focus();
        });
    }

    if (btnFinishEducation) {
        btnFinishEducation.addEventListener('click', async (e) => {
            e.preventDefault();

            const staffId = document.getElementById('sed_stfid').value;
            const staffName = document.getElementById('educationSelectedStaff').value;
            const tbody = educationTable.querySelector('tbody');
            const rows = tbody.querySelectorAll('tr');

            console.log('Finish button clicked - Staff ID:', staffId, 'Staff Name:', staffName);
            console.log('Number of education rows:', rows.length);

            if (!staffId || staffId === '0') {
                alert('Staff ID not set. Please save staff first.');
                console.error('Staff ID is empty or zero');
                return;
            }
            if (rows.length === 0) {
                alert('Please add at least one education record!');
                return;
            }

            const educationData = [];
            rows.forEach((row, index) => {
                const cells = row.querySelectorAll('td');
                if (cells.length < 4) return;

                educationData.push({
                    stfID: parseInt(staffId),
                    degreeName: cells[0].textContent,
                    yearpassing: cells[1].textContent,
                    instituteName: cells[2].textContent,
                    grade: cells[3].textContent
                });

                console.log(`Row ${index}:`, educationData[educationData.length - 1]);
            });

            const requestData = { records: educationData };

            try {
                console.log('Sending education data:', requestData);

                const resp = await fetch('/Admin/AdminDashboard/SaveStaffEducation', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify(requestData)
                });

                console.log('Education save response status:', resp.status);

                if (!resp.ok) {
                    throw new Error(`HTTP error! status: ${resp.status}`);
                }

                const result = await resp.json();
                console.log('Education save result:', result);

                if (result.success) {
                    alert('Education records added successfully!');
                    tbody.innerHTML = '';
                    switchWorkspace('viewStaff');
                } else {
                    alert(result.message || 'Failed to save education records');
                }
            } catch (error) {
                console.error('Error saving education records:', error);
                alert('Error saving education records: ' + error.message);
            }
        });
    }
}

async function loadStaffTable() {
    try {
        const res = await fetch('/Admin/AdminDashboard/GetAll');
        if (!res.ok) throw new Error('Network error');
        const data = await res.json();
        const tbody = document.querySelector('#staffTable tbody');
        if (!tbody) return;

        tbody.innerHTML = '';
        data.forEach((s, idx) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${idx + 1}</td>
                <td>${escapeHtml(s.stf_name)}</td>
                <td>${escapeHtml(s.stf_gender)}</td>
                <td>${escapeHtml(s.stf_staffId)}</td>
                <td>${escapeHtml(s.stf_designation)}</td>
                <td>${escapeHtml(s.stf_contact)}</td>
                <td>${s.stf_teacherorstaff ? 'Yes' : 'No'}</td>
            `;
            tr.addEventListener('click', () => loadEducationForStaff(s.stf_id));
            tbody.appendChild(tr);
        });
    } catch (err) {
        console.error('Error loading staff:', err);
    }
}

async function loadLatestStaff() {
    try {
        const response = await fetch('/Admin/AdminDashboard/GetLatestStaff');
        if (!response.ok) {
            console.warn('GetLatestStaff returned not ok');
            document.getElementById('sed_stfid').value = '';
            document.getElementById('educationSelectedStaff').value = '';
            return;
        }
        const data = await response.json();
        if (data && data.success) {
            document.getElementById('sed_stfid').value = data.stf_id || '';
            document.getElementById('educationSelectedStaff').value = `${data.stf_name || ''} (${data.stf_staffnumber || ''})`;
        } else {
            document.getElementById('sed_stfid').value = '';
            document.getElementById('educationSelectedStaff').value = '';
        }
    } catch (err) {
        console.error('Error fetching latest staff:', err);
        document.getElementById('sed_stfid').value = '';
        document.getElementById('educationSelectedStaff').value = '';
    }
}

async function loadEducationForStaff(stfId) {
    try {
        const resp = await fetch(`/Admin/StaffSetup/GetEducationByStaff/${encodeURIComponent(stfId)}`);
        if (!resp.ok) throw new Error('Failed to fetch education');
        const data = await resp.json();
        const container = document.getElementById('selectedStaffEducation');
        if (!container) return;

        container.innerHTML = '<h4>Education</h4>';
        if (!data || data.length === 0) {
            container.innerHTML += '<div>No education records found.</div>';
            return;
        }
        const table = document.createElement('table');
        table.style.width = '100%';
        table.border = '1';
        let html = '<thead><tr><th>Degree</th><th>Year</th><th>Institute</th><th>Grade</th></tr></thead><tbody>';
        data.forEach(r => {
            html += `<tr><td>${escapeHtml(r.degreeName)}</td><td>${escapeHtml(r.yearpassing)}</td><td>${escapeHtml(r.instituteName)}</td><td>${escapeHtml(r.grade)}</td></tr>`;
        });
        html += '</tbody>';
        table.innerHTML = html;
        container.appendChild(table);
    } catch (err) {
        console.error('Error loading education for staff:', err);
    }
}

// ========== CHALLAN MANAGEMENT ==========
function initializeChallanManagement() {
    const searchType = document.getElementById('searchType');
    const searchById = document.getElementById('searchById');
    const searchByBForm = document.getElementById('searchByBForm');
    const searchBtnId = document.getElementById('searchBtnId');
    const searchBtnBform = document.getElementById('searchBtnBform');

    if (searchType) {
        searchType.addEventListener('change', function () {
            searchById.classList.add('hidden');
            searchByBForm.classList.add('hidden');

            if (this.value === 'regNo') {
                searchById.classList.remove('hidden');
            } else if (this.value === 'bForm') {
                searchByBForm.classList.remove('hidden');
            }
        });
    }

    if (searchBtnId) {
        searchBtnId.addEventListener('click', () => {
            const appId = document.getElementById('app_idSearch').value;
            if (appId) {
                searchApplicant('regNo', appId);
            }
        });
    }

    if (searchBtnBform) {
        searchBtnBform.addEventListener('click', () => {
            const bForm = document.getElementById('app_BFORMNO').value;
            if (bForm) {
                searchApplicant('bForm', bForm);
            }
        });
    }
}

function searchApplicant(type, value) {
    const url = type === 'regNo'
        ? `/Admin/AdminDashboard/SearchApplicant?appId=${value}`
        : `/Admin/AdminDashboard/SearchApplicant?bForm=${encodeURIComponent(value)}`;

    fetch(url)
        .then(res => res.json())
        .then(data => {
            if (data.success && data.applicant) {
                renderApplicantInfo(data);
            } else {
                // Show error message - you might need to create an error container
                showMessage('❌ No applicant found with the provided details.', 'error');
            }
        })
        .catch(err => {
            console.error('Error searching applicant:', err);
            showMessage('Error searching applicant', 'error');
        });
}

function renderApplicantInfo(data) {
    // Create a container for the applicant info if it doesn't exist
    let container = document.getElementById('applicantInfoContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'applicantInfoContainer';
        // Insert after the search form
        const searchForm = document.getElementById('challanSearchForm');
        searchForm.parentNode.insertBefore(container, searchForm.nextSibling);
    }

    container.innerHTML = `
        <div class="success-message">✅ Applicant found successfully!</div>
        <table id="applicantTable" style="width: 100%; border-collapse: collapse; margin-top: 10px;">
            <thead>
                <tr>
                    <th>App ID</th>
                    <th>Name</th>
                    <th>B-Form Number</th>
                    <th>DOB</th>
                    <th>Father Name</th>
                    <th>Father Contact</th>
                    <th>Applied for</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>${data.applicant.id}</td>
                    <td>${escapeHtml(data.applicant.fullName)}</td>
                    <td>${escapeHtml(data.applicant.bFormNumber)}</td>
                    <td>${formatDate(data.applicant.dateOfBirth)}</td>
                    <td>${escapeHtml(data.guardian?.fullName || 'N/A')}</td>
                    <td>${escapeHtml(data.guardian?.contactNumber || 'N/A')}</td>
                    <td>${escapeHtml(data.classInfo?.description || 'N/A')}</td>
                </tr>
            </tbody>
        </table>
        <div class="field" id="challanTypeContainer">
            <label>Select Challan type</label>
            <select id="challanType" required>
                <option value="">---Select---</option>
                <option value="Admission Charges">Admission Fee Challan</option>
                <option value="Class Fee Charges">Class Fee Challan</option>
            </select>
        </div>
    `;

    const challanType = document.getElementById('challanType');
    if (challanType) {
        challanType.addEventListener('change', function () {
            loadChallanForm(this.value, data);
        });
    }
}

// Helper functions
function formatDate(dateString) {
    if (!dateString) return 'N/A';
    try {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-GB');
    } catch {
        return 'N/A';
    }
}

function showMessage(message, type) {
    // Create or find message container
    let messageContainer = document.getElementById('messageContainer');
    if (!messageContainer) {
        messageContainer = document.createElement('div');
        messageContainer.id = 'messageContainer';
        const searchForm = document.getElementById('challanSearchForm');
        searchForm.parentNode.insertBefore(messageContainer, searchForm.nextSibling);
    }

    messageContainer.innerHTML = `<div class="${type === 'error' ? 'error-message' : 'success-message'}">${message}</div>`;

    // Auto remove after 5 seconds
    setTimeout(() => {
        messageContainer.innerHTML = '';
    }, 5000);
}

function loadChallanForm(challanType, applicantData) {
    const challanContainer = document.getElementById('challanFormContainer');
    const classId = challanType === 'class' ? applicantData.applicant.appliedForClassId : null;

    challanContainer.innerHTML = '<div>Loading fee details...</div>';
    challanContainer.classList.remove('hidden');

    const url = challanType === 'Admission Charges'
        ? '/Admin/AdminDashboard/GetFeeHeadsC'
        : `/Admin/AdminDashboard/GetFeeHeads/${classId}`;

    fetch(url)
        .then(response => {
            if (!response.ok) throw new Error('Failed to fetch fee data');
            return response.json();
        })
        .then(data => {
            renderChallanForm(data, challanType, applicantData);
        })
        .catch(err => {
            console.error('Error fetching fee data:', err);
            challanContainer.innerHTML = `<div class="error-message">Error loading fee data: ${err.message}</div>`;
        });
}

function renderChallanForm(feeData, challanType, applicantData) {
    const challanContainer = document.getElementById('challanFormContainer');

    let html = `
        <form id="challanGenerateForm" method="post" action="/Admin/AdminDashboard/GenerateChallan" enctype="multipart/form-data" style="width:100%;">
            <input type="hidden" name="GeneratedChallan.challanType" value="${challanType}" />
            <input type="hidden" name="GeneratedChallan.app_id" value="${applicantData.applicant.id}" />
            <div class="flex-row" style="display:flex;">
                <div style="width:35%;">
                    <div class="field">
                        <label>Applicant ID</label>
                        <input type="text" value="${applicantData.applicant.id}" readonly class="fee-input" />
                    </div>
                    <br>
                    <div class="field">
                        <label>Due Date of Submission</label>
                        <input name="GeneratedChallan.dueDate" type="date" id="dueDate" required min="${new Date().toISOString().split('T')[0]}" />
                    </div>
                    <br>
                    <div class="field">
                        <label>No of Days after Due Date</label>
                        <input name="GeneratedChallan.fineDate" type="number" min="1" max="30" step="1" id="expiryDate" placeholder="e.g., 7" required />
                    </div>
                </div>
                <div style="width:65%;" id="challanDetailsBox"></div>
            </div>
            <button type="submit" id="generateChallanBtn" style="margin-top: 16px; padding: 8px 16px; background: #9e1f23; color: white; border: none; border-radius: 6px; cursor: pointer;">Generate Challan</button>
        </form>
    `;

    challanContainer.innerHTML = html;

    const challanDetailsBox = document.getElementById('challanDetailsBox');
    let totalAmount = 0;

    // File Upload Section
    const fileWrapper = document.createElement('div');
    fileWrapper.style.margin = '15px 0';
    fileWrapper.innerHTML = `
        <label><b>Upload Supporting Document (Optional)</b></label><br>
        <input type="file" id="discountFile" name="DiscountFile" accept=".pdf,.jpg,.png,.jpeg"/>
        <p style="color:grey; font-size:12px;">* Upload document if required for verification</p>
    `;
    challanDetailsBox.appendChild(fileWrapper);

    // Fee Heads Section
    const feeHeader = document.createElement('h4');
    feeHeader.textContent = 'Fee Details';
    feeHeader.style.marginBottom = '15px';
    challanDetailsBox.appendChild(feeHeader);

    feeData.forEach((item, index) => {
        const wrapper = document.createElement('div');
        wrapper.classList.add('fee-row');
        wrapper.innerHTML = `
        <input type="text" class="fee-input" style="flex: 2;" value="${escapeHtml(item.feeHead)}" readonly />
        <input type="number" class="fee-input" style="flex: 1;" name="GeneratedChallan.FeeHeads[${index}].Value" value="${item.feeAmount}" readonly id="amount-${index}" />
        <input type="hidden" name="GeneratedChallan.FeeHeads[${index}].Key" value="${escapeHtml(item.feeHead)}" />
        <input type="hidden" name="GeneratedChallan.FeeHeads[${index}].FacId" value="${item.facId}" />
        <input type="hidden" name="GeneratedChallan.FeeHeads[${index}].FcmId" value="${item.fcmId}" />
        <button type="button" id="edit-${index}" class="edit-btn disabled">Edit</button>
    `;
        challanDetailsBox.appendChild(wrapper);

        totalAmount += item.feeAmount || 0;

        // Edit toggle functionality
        document.getElementById(`edit-${index}`).addEventListener('click', function () {
            const input = document.getElementById(`amount-${index}`);
            if (this.textContent === 'Edit' && !this.classList.contains('disabled')) {
                input.removeAttribute('readonly');
                input.focus();
                this.textContent = 'Save';
                this.classList.remove('enabled');
                this.classList.add('save');
            } else if (this.textContent === 'Save') {
                // Validate the amount before saving
                const newAmount = parseInt(input.value) || 0;
                if (newAmount < 0) {
                    showMessage('Amount cannot be negative', 'error');
                    input.value = item.feeAmount; // Reset to original value
                    return;
                }

                input.setAttribute('readonly', true);
                this.textContent = 'Edit';
                this.classList.remove('save');
                this.classList.add('enabled');
                updateChallanTotalAmount();
            }
        });

        // Add input validation for amount field
        document.getElementById(`amount-${index}`).addEventListener('blur', function () {
            if (!this.hasAttribute('readonly')) {
                const value = parseInt(this.value) || 0;
                if (value < 0) {
                    showMessage('Amount cannot be negative', 'error');
                    this.value = item.feeAmount; // Reset to original value
                }
            }
        });
    });

    // Total Amount Display
    const totalWrapper = document.createElement('div');
    totalWrapper.style.marginTop = '20px';
    totalWrapper.style.padding = '10px';
    totalWrapper.style.backgroundColor = '#f5f5f5';
    totalWrapper.style.borderRadius = '5px';
    totalWrapper.innerHTML = `<strong>Total Amount: Rs. <span id="totalAmount">${totalAmount}</span></strong>`;
    challanDetailsBox.appendChild(totalWrapper);

    const totalInput = document.createElement('input');
    totalInput.type = 'hidden';
    totalInput.name = 'TotalAmount';
    totalInput.value = totalAmount;
    challanDetailsBox.appendChild(totalInput);

    // Enable editing after file upload
    document.getElementById('discountFile').addEventListener('change', function () {
        const canEdit = this.files.length > 0;
        feeData.forEach((_, index) => {
            const editBtn = document.getElementById(`edit-${index}`);
            if (canEdit) {
                editBtn.disabled = false;
                editBtn.classList.remove('disabled');
                editBtn.classList.add('enabled');
            } else {
                editBtn.disabled = true;
                editBtn.classList.remove('enabled');
                editBtn.classList.add('disabled');
            }
        });
    });
    const challanForm = document.getElementById('challanGenerateForm');
    // ========== UPDATED FORM SUBMISSION ==========
    document.getElementById('challanGenerateForm').addEventListener('submit', function (e) {
        e.preventDefault(); // Prevent default form submission

        const dueDate = document.getElementById('dueDate').value;
        const fineDate = document.getElementById('expiryDate').value;

        if (!dueDate) {
            alert('Please select a due date');
            return;
        }

        if (!fineDate || fineDate < 1) {
            alert('Please enter valid number of days for fine calculation');
            return;
        }

        const selectedDate = new Date(dueDate);
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (selectedDate < today) {
            alert('Due date cannot be in the past');
            return;
        }

        // Show loading state
        const generateBtn = document.getElementById('generateChallanBtn');
        generateBtn.disabled = true;
        generateBtn.textContent = 'Generating Challan...';
       
        // Create FormData object
        const formData = new FormData(challanForm);

        // Add anti-forgery token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (token) {
            formData.append('__RequestVerificationToken', token);
        }

        // Submit via AJAX
        fetch('/Admin/AdminDashboard/GenerateChallan', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest' // Identify as AJAX request
            }
        })
            .then(async response => {
                const text = await response.text();
                if (!response.ok) {
                    console.error('Server returned:', text);
                    throw new Error(text);
                }
                return JSON.parse(text);
            })
            .then(data => {
                if (data.success) {
                    // Hide challan form section and show generated challan section
                    document.getElementById('challanForm').style.display = 'none';
                    document.getElementById('applicantInfoContainer').style.display = 'none';
                    document.getElementById('challanFormContainer').classList.add('hidden');

                    // Switch to generated challan section
                    switchWorkspace('generatedChallan');

                    // Load the generated challan HTML into the container
                    const printChallan = document.getElementById('printChallan');
                    if (printChallan) {
                        renderGeneratedChallan(data.data);
                    }

                    showMessage('Challan generated successfully!', 'success');
                } else {
                    throw new Error(data.message || 'Failed to generate challan');
                }
            })
            .catch(err => {
                console.error('Error generating challan:', err);
                showMessage('Error generating challan: ' + err.message, 'error');
            })
            .finally(() => {
                generateBtn.disabled = false;
                generateBtn.textContent = 'Generate Challan';
            });
    });
}

function renderGeneratedChallan(data) {
    const challans = document.querySelectorAll('.challan');

    challans.forEach(challan => {

        challan.querySelector('#schoolName').textContent = data.schoolName;
        challan.querySelector('#bankName').textContent = data.bankName;
        challan.querySelector('#branchLocation').textContent = data.branchLocation;

        challan.querySelector('#accountTitle').textContent = `Account Title: ${data.accountTitle}`;
        challan.querySelector('#accountNumber').textContent = data.accountNumber;

        challan.querySelector('#studentName').textContent = data.applicantName;
        challan.querySelector('#guardianName').textContent = data.guardianName;
        challan.querySelector('#regNo').textContent = data.registrationNo;
        challan.querySelector('#className').textContent = data.className;

        challan.querySelector('#totalAmount').textContent = data.totalAmount;
        challan.querySelector('#payableBeforeDue').textContent = data.totalAmount;

        challan.querySelector('#dueDate').textContent =
            new Date(data.dueDate).toLocaleDateString();

        challan.querySelector('#issueDate').textContent =
            new Date().toLocaleDateString();

        challan.querySelector('#expiryDate').textContent =
            new Date(data.dueDate)
                .setDate(new Date(data.dueDate).getDate() + data.fineDays);
    });

    // Fee Heads (same for all copies)
    const feeBody = document.getElementById('feeTableBody');
    feeBody.innerHTML = '';

    data.feeHeads.forEach(fee => {
        feeBody.innerHTML += `
            <tr>
                <td style="width:65%;border:1px solid black;text-align:left">${fee.feeName}</td>
                <td style="width:35%;border:1px solid black;text-align:left">${fee.amount}</td>
            </tr>
        `;
    });
}

function updateChallanTotalAmount() {
    let total = 0;
    const amountInputs = document.querySelectorAll('[id^="amount-"]');
    amountInputs.forEach(input => {
        total += parseInt(input.value) || 0;
    });
    const totalSpan = document.getElementById('totalAmount');
    if (totalSpan) {
        totalSpan.textContent = total;
    }
    const totalInput = document.querySelector('input[name="TotalAmount"]');
    if (totalInput) {
        totalInput.value = total;
    }
}

// ========== UTILITY FUNCTIONS ==========
function escapeHtml(str) {
    if (!str && str !== 0) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

// ========== DOWNLOAD FUNCTIONALITY ==========
// PDF Download for Challan
document.addEventListener("click", function (e) {
    if (e.target && e.target.id === "downloadBtn") {
        console.log("✅ Download button clicked!");
        const element = document.getElementById("printChallan");
        if (element) {
            html2pdf().from(element).save("ChallanForm.pdf");
        } else {
            console.error("❌ printChallan element not found!");
        }
    }
});

// PDF Download for Forms (Admission Form, etc.)
document.addEventListener("click", function (e) {
    if (e.target && e.target.id === "downloadFormBtn") {
        console.log("✅ Download form button clicked!");
        const element = document.getElementById("printSection");
        if (element) {
            html2pdf().from(element).save("AdmissionForm.pdf");
        } else {
            console.error("❌ printSection element not found!");
        }
    }
});

// View/Hide Form Toggle
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
function onClassChange() {
    console.log("Class changed, fetching teachers...");
    const classId = document.getElementById("allocateClass").value;

    if (classId === "") return;

    fetch(`/Admin/TeacherAllocation/GetTeachersByClass?classId=${classId}`)
        .then(response => response.json())
        .then(data => {
            console.log(data); // handle response here
        })
        .catch(error => {
            console.error("Error:", error);
        });
}

// Make functions globally available
window.switchWorkspace = switchWorkspace;
window.editFee = editFee;
window.deleteFee = deleteFee;
window.editClassFee = editClassFee;
window.deleteClassFee = deleteClassFee;
window.createFeeFromTable = createFeeFromTable;
window.findTeacher = findTeacher;
window.editSubject = editSubject;
window.deleteSubject = deleteSubject;
