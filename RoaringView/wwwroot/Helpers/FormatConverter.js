// date-format.js

function formatDateInput(inputId) {
    const input = document.getElementById(inputId);
    if (input) {
        const dateValue = input.value;
        if (dateValue) {
            const parts = dateValue.split('-');
            if (parts.length === 3) {
                const formattedDate = `${parts[2]}/${parts[1]}/${parts[0]}`;
                input.value = formattedDate;
            }
        }
    }
}
