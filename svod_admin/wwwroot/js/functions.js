function deleteTarg(login, button) {
    fetch("/Target/TargetUser?handler=Delete&login=" + login)
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            const modalEl = button.closest(".modal"); //закрываем модальное окно
            const modalInstance = bootstrap.Modal.getInstance(modalEl);
            if (modalInstance) {
                modalInstance.hide();
            }

            if (data.result) {
                const deleteItem = button.closest("tr"); //удаляем строку
                deleteItem.remove();
            }
            showToast(data.result, data.message);
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast("Произошла ошибка при удалении", false);
        });
}

function deleteSubj(login, button) {
    fetch("/Subject/SubjectUsers?handler=Delete&login=" + login)
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            const modalEl = button.closest(".modal"); //закрываем модальное окно
            const modalInstance = bootstrap.Modal.getInstance(modalEl);
            if (modalInstance) {
                modalInstance.hide();
            }

            if (data.result) {
                const deleteItem = button.closest("tr"); //удаляем строку
                deleteItem.remove();
            }
            showToast(data.result, data.message);
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast("Произошла ошибка при удалении", false);
        });
}

function deleteTerr(login, territory, button) {
    fetch("/Territory/TerritoryUsers?handler=Delete&login=" + login + "&territory=" + territory)
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            const modalEl = button.closest(".modal"); //закрываем модальное окно
            const modalInstance = bootstrap.Modal.getInstance(modalEl);
            if (modalInstance) {
                modalInstance.hide();
            }

            if (data.result) {
                const deleteItem = button.closest("tr"); //удаляем строку
                deleteItem.remove();
            }
            showToast(data.result, data.message);
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast("Произошла ошибка при удалении", false);
        });
}

function saveSubF(login, formid, subjectid, num) {
    const selectElement = document.querySelector(`#Permission_${formid}`); 
    const permission = selectElement?.value;

    fetch("/Subject/SubjectFinegrained?handler=Edit&login=" + login + "&formid=" + formid + "&subjectid=" + subjectid + "&permission=" + permission + "&num=" + num) // 
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            showToast(data.result, data.message);
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast(false, "Произошла ошибка при сохранении.");
        });
}

function saveTarF(login, formid, num) {
    const selectElement = document.querySelector(`#Permission_${formid}`);
    const permission = selectElement?.value;

    fetch("/Target/TargetFinegrained?handler=Edit&login=" + login + "&formid=" + formid + "&permission=" + permission + "&num=" + num)
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            showToast(data.result, data.message);
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast(false, "Произошла ошибка при сохранении.");
        });
}

function saveTerF(login, formid, territoryid, num) {
    const selectElement = document.querySelector(`#Permission_${formid}`);
    const permission = selectElement?.value;

    fetch("/Territory/TerritoryFinegrained?handler=Edit&login=" + login + "&formid=" + formid + "&territoryid=" + territoryid + "&permission=" + permission + "&num=" + num)
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            showToast(data.result, data.message);
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast(false, "Произошла ошибка при сохранении.");
        });
}

function saveSubject(id) {
    const form = document.getElementById('SubForm');
    const formData = new FormData(form);

    fetch("/RegisterSubject/CreateSubject?handler=Create&id=" + id, {
        method: 'POST',
            headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: formData
    })
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            sessionStorage.setItem("toastResult", data.result);
            sessionStorage.setItem("toastMessage", data.message);

            window.location.href = "/RegisterSubject/RegisterSubject";
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast(false, "Произошла ошибка при сохранении.");
        });
}

function editSubject(id) {
    const form = document.getElementById('SubForm');
    const formData = new FormData(form);

    fetch("/RegisterSubject/EditSubject/"+ id +"?handler=Save&id=" + id, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: formData
    })
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            sessionStorage.setItem("toastResult", data.result);
            sessionStorage.setItem("toastMessage", data.message);

            window.location.href = "/RegisterSubject/RegisterSubject";
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast(false, "Произошла ошибка при сохранении.");
        });
}

function showToast(result, message) {
    const existing = document.getElementById("bottomToast");
    if (existing) { // Удаляем предыдущий тост, если есть
        existing.remove();
    }

    const toastContainer = $(".toast-container");
    toastContainer.empty();

    const toastHtml = `
                    <div class="toast text-white ${result ? 'bg-success' : 'bg-danger'} border-0"
                         id="bottomToast"
                         role="alert"
                         aria-live="assertive"
                         aria-atomic="true">
                        <div class="d-flex">
                            <div class="toast-body">
                                ${message}
                            </div>
                            <button type="button"
                                    class="btn-close btn-close-white me-2 m-auto"
                                    data-bs-dismiss="toast"
                                    aria-label="Close"></button>
                        </div>
                    </div>`;

    toastContainer.append(toastHtml);
    const toastEl = document.getElementById("bottomToast");
    const toast = new bootstrap.Toast(toastEl);
    toast.show();
};

function toggleClosedCompanies(checkbox) {
    const table = document.getElementById("tuser-tbl");
    const oldTbody = table.querySelector("tbody");

    let path = "";
    if (checkbox.checked) {
        path = "/RegisterSubject/RegisterSubject?handler=ShowCloseSub";

    } else {
        path = "/RegisterSubject/RegisterSubject?handler=ShowOpenSub";
    }
    fetch(path)
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            oldTbody.remove();
            const newTbody = document.createElement("tbody");

            let rows = "";
            data.list.forEach((sub) => {
                rows += `
                        <tr>
                            <td>${sub.subjectID}</td>
                            <td>${sub.masterID !== 0 ? sub.masterID + "-" + sub.masterName : ""}</td>
                            <td>${sub.subjectName}</td>
                            <td>${sub.ogrn}</td>
                            <td>${sub.kpp}</td>
                            <td>${sub.inn}</td>
                            <td>${sub.okpo}</td>
                            <td>${sub.territoryWork}</td>
                            <td>${sub.okved}</td>
                            <td>${sub.sinceDate}</td>
                            <td>${sub.uptoDate}</td>
                            <td>${sub.username}</td>
                            <td>${sub.changeDate}</td>
                            <td>
                                <form method="get" action="/RegisterSubject/EditSubject/${sub.subjectID}">
                                    <button type="submit" class="btn btn-outline-success actions" title="Изменить">
                                        <span>
                                            <img src="/pencil-square.svg" />
                                        </span>
                                    </button>
                                </form>
                            </td>
                        </tr>
                `;
            });
            newTbody.innerHTML = rows;
            table.appendChild(newTbody);
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast(false, "Произошла ошибка при выборе предприятий.");
        });
}