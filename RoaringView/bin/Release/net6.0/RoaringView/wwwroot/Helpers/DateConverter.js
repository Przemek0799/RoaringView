function initializeFlatpickr(elementId, dotNetReference, methodName) {
    flatpickr(document.getElementById(elementId), {
        dateFormat: "d/m/Y",
        onChange: function (selectedDates, dateStr, instance) {
            dotNetReference.invokeMethodAsync(methodName, dateStr);
        },
        onReady: function (dateObj, dateStr, instance) {
            instance.input.classList.add("form-control");
            instance.input.style.backgroundColor = "#fff"; // Set white background
        }
    });
}
