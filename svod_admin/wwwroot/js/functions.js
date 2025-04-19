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

function saveTarF(login, formid, num) {
    fetch("/Target/TargetFinegrained?handler=Edit&login=" + login + "&formid=" + formid + "&num=" + num)
        .then(response => {
            if (!response.ok) throw new Error("Network error");
            return response.json();
        })
        .then(data => {
            showToast(data.result, data.message);
        })
        .catch(error => {
            console.error("Ошибка:", error);
            showToast(false, "Произошла ошибка при удалении");
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