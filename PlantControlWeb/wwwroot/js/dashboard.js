const DEVICE_ID = 1;

let dashboardLoading = false;

async function refreshDashboard() {
    if (dashboardLoading) {
        return;
    }

    dashboardLoading = true;

    try {
        await loadDashboard();
    }
    finally {
        dashboardLoading = false;
    }
}

async function loadDashboard() {

    const errorElement =
        document.getElementById("dashboard-error");

    if (errorElement) {
        errorElement.hidden = true;
    }

    try {

        const response = await fetch(
            `/api/Devices/${DEVICE_ID}/dashboard`
        );

        if (!response.ok) {
            throw new Error(
                `HTTP ${response.status}`
            );
        }

        const data = await response.json();

        console.log("Dashboard:", data);

        updateDevice(data.device);
        updateEnvironment(data.environment, data.profile);
        updateActuators(data.actuators);
        updateAlerts(data.alerts);

    }
    catch (error) {

        console.error(
            "Error cargando dashboard:",
            error
        );

        if (errorElement) {
            errorElement.hidden = false;
        }
    }
}

function updateDevice(device) {

    document.getElementById("device-name").textContent =
        device.name;

    document.getElementById("device-uid").textContent =
        device.deviceUid;

    document.getElementById("firmware-version").textContent =
        device.firmwareVersion ?? "---";

    document.getElementById("device-status-text").textContent =
        device.status.toUpperCase();

    updateStatusStyle(device.status);

    if (device.secondsSinceLastSeen !== null) {

        const seconds =
            Math.round(device.secondsSinceLastSeen);

        document.getElementById("last-seen").textContent =
            `Hace ${seconds} segundos`;

    }
    else {

        document.getElementById("last-seen").textContent =
            "Nunca";

    }
}

function updateStatusStyle(status) {

    const container =
        document.getElementById("device-status");

    const dot =
        container.querySelector(".status-dot");

    container.classList.remove(
        "status-online",
        "status-warning",
        "status-offline"
    );

    dot.classList.remove(
        "dot-online",
        "dot-warning",
        "dot-offline"
    );

    if (status === "online") {

        container.classList.add("status-online");
        dot.classList.add("dot-online");

    }
    else if (status === "warning") {

        container.classList.add("status-warning");
        dot.classList.add("dot-warning");

    }
    else {

        container.classList.add("status-offline");
        dot.classList.add("dot-offline");

    }
}

function updateEnvironment(environment, profile) {

    if (!environment) {
        return;
    }

    // Temperatura del aire
    updateSensor(
        "temperature-air",
        "temperature-air-status",
        "temperature-air-progress",
        "temperature-air-range",
        environment.temperatureAir,
        profile.temperatureAirMin,
        profile.temperatureAirMax,
        "°C"
    );

    // Temperatura del sustrato
    updateSensor(
        "temperature-soil",
        "temperature-soil-status",
        "temperature-soil-progress",
        "temperature-soil-range",
        environment.temperatureSoil,
        profile.temperatureSoilMin,
        profile.temperatureSoilMax,
        "°C"
    );

    // Humedad ambiental
    updateSensor(
        "humidity",
        "humidity-status",
        "humidity-progress",
        "humidity-range",
        environment.humidity,
        profile.humidityMin,
        profile.humidityMax,
        "%"
    );

    // Humedad del sustrato
    updateSensor(
        "soil-moisture",
        "soil-moisture-status",
        "soil-moisture-progress",
        "soil-moisture-range",
        environment.soilMoisture,
        profile.soilMoistureMin,
        profile.soilMoistureMax,
        "%"
    );

    // Iluminación
    updateSensor(
        "light",
        "light-status",
        "light-progress",
        "light-range",
        environment.light,
        profile.lightMin,
        profile.lightMax,
        "lux"
    );
}

function updateSensor(
    valueId,
    statusId,
    progressId,
    rangeId,
    value,
    min,
    max,
    unit
) {

    const valueElement = document.getElementById(valueId);
    const statusElement = document.getElementById(statusId);
    const progressElement = document.getElementById(progressId);
    const rangeElement = document.getElementById(rangeId);

    if (!valueElement ||
        !statusElement ||
        !progressElement ||
        !rangeElement) {
        return;
    }

    // Sin lectura válida
    if (value === null || value === undefined) {

        valueElement.textContent = "--";
        statusElement.textContent = "Sin datos";
        progressElement.style.width = "0%";
        rangeElement.textContent = "Rango: --";

        statusElement.className = "sensor-status";
        progressElement.className = "sensor-progress-bar";

        return;
    }

    // Mostrar valor
    valueElement.textContent = Number(value).toFixed(1);

    // Mostrar rango
    if (min !== null && min !== undefined &&
        max !== null && max !== undefined) {

        rangeElement.textContent =
            `Rango: ${min} - ${max} ${unit}`;
    }
    else {

        rangeElement.textContent = "Rango: --";
    }

    // Determinar estado
    let status;

    if (min !== null && min !== undefined &&
        value < min) {

        status = "Bajo";

    }
    else if (max !== null && max !== undefined &&
        value > max) {

        status = "Alto";

    }
    else {

        status = "Normal";
    }

    statusElement.textContent = status;

    // Calcular posición dentro del rango
    let percentage = 50;

    if (min !== null && max !== null &&
        max > min) {

        percentage =
            ((value - min) / (max - min)) * 100;

        percentage =
            Math.max(0, Math.min(100, percentage));
    }

    progressElement.style.width = `${percentage}%`;

    // Limpiar clases anteriores
    statusElement.className = "sensor-status";
    progressElement.className = "sensor-progress-bar";

    // Aplicar estado visual
    if (status === "Bajo") {

        statusElement.classList.add("sensor-status-warning");
        progressElement.classList.add("sensor-progress-warning");

    }
    else if (status === "Alto") {

        statusElement.classList.add("sensor-status-danger");
        progressElement.classList.add("sensor-progress-danger");

    }
    else {

        statusElement.classList.add("sensor-status-normal");
        progressElement.classList.add("sensor-progress-normal");
    }
}

function updateActuators(actuators) {

    const container =
        document.getElementById("actuators-container");

    if (!container) {
        return;
    }

    if (!actuators || actuators.length === 0) {

        container.innerHTML = `
            <div class="loading-message">
                No hay actuadores configurados.
            </div>
        `;

        return;
    }

    container.innerHTML = "";

    actuators.forEach(actuator => {

        const icon = getActuatorIcon(actuator.type);

        const state = actuator.state === true;

        const actuatorElement =
            document.createElement("div");

        actuatorElement.className =
            `actuator ${state ? "actuator-active" : ""}`;

        actuatorElement.innerHTML = `

            <div class="actuator-info">

                <div class="actuator-icon">
                    ${icon}
                </div>

                <div>

                    <strong>
                        ${actuator.name}
                    </strong>

                    <small>
                        GPIO ${actuator.gpio ?? "--"}
                    </small>

                </div>

            </div>


            <div class="actuator-control">

                <div class="actuator-state ${state ? "active" : ""}">

                    <span></span>

                    <strong>
                        ${state ? "ENCENDIDO" : "APAGADO"}
                    </strong>

                </div>


                <button
                    class="actuator-toggle ${state ? "active" : ""}"
                    data-actuator-id="${actuator.id}"
                    ${actuator.enabled ? "" : "disabled"}
                    aria-label="Controlar ${actuator.name}"
                >

                    <span class="toggle-knob"></span>

                </button>

            </div>
        `;

        const button =
            actuatorElement.querySelector(".actuator-toggle");

        if (button) {

            button.addEventListener(
                "click",
                () => toggleActuator(
                    actuator.id,
                    !state
                )
            );
        }

        container.appendChild(actuatorElement);
    });
}

async function toggleActuator(actuatorId, state) {

    try {

        const response = await fetch(
            `/api/Actuators/${actuatorId}/control`,
            {
                method: "POST",

                headers: {
                    "Content-Type": "application/json"
                },

                body: JSON.stringify({
                    state: state,
                    pwm: null,
                    reason: "Control manual desde dashboard"
                })
            }
        );


        if (!response.ok) {

            const error =
                await response.text();

            throw new Error(error);
        }


        // Volvemos a cargar el dashboard
        // para obtener el estado real guardado.
        await loadDashboard();

    }
    catch (error) {

        console.error(
            "Error al controlar actuador:",
            error
        );

        alert(
            "No fue posible controlar el actuador."
        );
    }
}

function getActuatorIcon(type) {

    switch (type) {

        case "fan":
            return "🌀";

        case "pump":
            return "💦";

        case "humidifier":
            return "💨";

        case "uv_light":
            return "💡";

        default:
            return "⚙️";
    }
}

function updateAlerts(alerts) {

    const container =
        document.getElementById("alerts-container");

    if (!container) {
        return;
    }


    // No hay alertas activas
    if (!alerts || alerts.length === 0) {

        container.innerHTML = `
            <div class="no-alerts">
                <span class="no-alerts-icon">✓</span>

                <div>
                    <strong>Sistema dentro de parámetros</strong>

                    <span>
                        No hay alertas ambientales activas.
                    </span>
                </div>
            </div>
        `;

        return;
    }


    container.innerHTML = "";


    alerts.forEach(alert => {

        const severity =
            (alert.severity || "warning").toLowerCase();

        const title =
            getAlertTitle(alert.type);

        const icon =
            getAlertIcon(severity);

        const createdAt =
            formatDateTime(alert.createdAt);


        const alertElement =
            document.createElement("div");


        alertElement.className =
            `alert alert-${severity}`;


        alertElement.innerHTML = `

            <div class="alert-icon">
                ${icon}
            </div>


            <div class="alert-content">

                <div class="alert-top">

                    <strong>
                        ${title}
                    </strong>

                    <span class="alert-severity">
                        ${getSeverityLabel(severity)}
                    </span>

                </div>


                <p>
                    ${alert.message}
                </p>


                <small class="alert-time">
                    ${createdAt}
                </small>

            </div>
        `;


        container.appendChild(alertElement);
    });
}

function getAlertIcon(severity) {

    switch (severity) {

        case "danger":
        case "critical":
            return "⚠️";

        case "warning":
            return "⚠";

        default:
            return "ℹ️";
    }
}

function getSeverityLabel(severity) {
    switch (severity) {
        case "danger":
        case "critical":
            return "CRÍTICA";

        case "warning":
            return "ADVERTENCIA";

        case "info":
            return "INFORMACIÓN";

        default:
            return severity.toUpperCase();
    }
}

function formatDateTime(dateString) {

    if (!dateString) {
        return "--";
    }

    const date =
        new Date(dateString);

    return date.toLocaleString(
        "es-MX",
        {
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit"
        }
    );
}

function getAlertTitle(type) {

    const titles = {

        temperature_air_low:
            "Temperatura del aire baja",

        temperature_air_high:
            "Temperatura del aire alta",

        temperature_soil_low:
            "Temperatura del sustrato baja",

        temperature_soil_high:
            "Temperatura del sustrato alta",

        humidity_low:
            "Humedad ambiental baja",

        humidity_high:
            "Humedad ambiental alta",

        soil_moisture_low:
            "Humedad del sustrato baja",

        soil_moisture_high:
            "Humedad del sustrato alta",

        light_low:
            "Iluminación baja",

        light_high:
            "Iluminación alta"
    };

    return titles[type] ?? "Alerta ambiental";
}

let temperatureChart = null;
let environmentChart = null;

async function loadHistory(hours = 24) {

    try {

        const response = await fetch(
            `/api/EnvironmentMeasurements/device/${DEVICE_ID}?hours=${hours}`
        );

        if (!response.ok) {

            throw new Error(
                `HTTP ${response.status}`
            );

        }

        const measurements =
            await response.json();

        console.log(
            "Historial:",
            measurements
        );

        renderEnvironmentChart(measurements);

    }
    catch (error) {

        console.error(
            "Error cargando historial:",
            error
        );

    }
}

function renderEnvironmentChart(measurements) {

    const temperatureCanvas =
        document.getElementById("temperature-chart");

    const environmentCanvas =
        document.getElementById("environment-chart");

    if (!temperatureCanvas || !environmentCanvas) {
        return;
    }

    /*
     * Si no existen mediciones, mostramos el canvas
     * pero evitamos intentar crear gráficas vacías.
     */
    if (!measurements || measurements.length === 0) {

        if (temperatureChart) {
            temperatureChart.destroy();
            temperatureChart = null;
        }

        if (environmentChart) {
            environmentChart.destroy();
            environmentChart = null;
        }

        return;
    }


    /*
     * Preparar etiquetas de tiempo
     */
    const labels = measurements.map(m =>
        formatTime(m.recordedAt)
    );


    /*
     * =========================
     * TEMPERATURA
     * =========================
     */

    const temperatureAir =
        measurements.map(m => m.temperatureAir);

    const temperatureSoil =
        measurements.map(m => m.temperatureSoil);


    if (temperatureChart) {
        temperatureChart.destroy();
    }

    temperatureChart =
        new Chart(temperatureCanvas, {

            type: "line",

            data: {

                labels,

                datasets: [

                    {
                        label: "Temperatura del aire",

                        data: temperatureAir,

                        tension: 0.3,

                        pointRadius: 0,

                        pointHoverRadius: 5,

                        borderWidth: 2
                    },

                    {
                        label: "Temperatura del sustrato",

                        data: temperatureSoil,

                        tension: 0.3,

                        pointRadius: 0,

                        pointHoverRadius: 5,

                        borderWidth: 2
                    }

                ]

            },

            options: {

                responsive: true,

                maintainAspectRatio: false,

                interaction: {
                    mode: "index",
                    intersect: false
                },

                plugins: {

                    legend: {
                        position: "top",

                        labels: {
                            color: "#94a3b8",
                            usePointStyle: true,
                            boxWidth: 8
                        }
                    },

                    tooltip: {

                        callbacks: {

                            label: function(context) {

                                const value =
                                    context.parsed.y;

                                return `${context.dataset.label}: ${value} °C`;
                            }

                        }

                    }

                },

                scales: {

                    x: {

                        ticks: {
                            color: "#94a3b8",
                            maxTicksLimit: 8
                        },

                        grid: {
                            color: "rgba(148, 163, 184, 0.08)"
                        }

                    },

                    y: {

                        title: {
                            display: true,
                            text: "Temperatura (°C)",
                            color: "#94a3b8"
                        },

                        ticks: {
                            color: "#94a3b8"
                        },

                        grid: {
                            color: "rgba(148, 163, 184, 0.08)"
                        }

                    }

                }

            }

        });


    /*
     * =========================
     * HUMEDAD + ILUMINACIÓN
     * =========================
     */

    const humidity =
        measurements.map(m => m.humidity);

    const soilMoisture =
        measurements.map(m => m.soilMoisture);

    const light =
        measurements.map(m => m.light);


    if (environmentChart) {
        environmentChart.destroy();
    }


    environmentChart =
        new Chart(environmentCanvas, {

            type: "line",

            data: {

                labels,

                datasets: [

                    {
                        label: "Humedad",

                        data: humidity,

                        tension: 0.3,

                        pointRadius: 0,

                        pointHoverRadius: 5,

                        borderWidth: 2,

                        yAxisID: "y"
                    },

                    {
                        label: "Humedad del sustrato",

                        data: soilMoisture,

                        tension: 0.3,

                        pointRadius: 0,

                        pointHoverRadius: 5,

                        borderWidth: 2,

                        yAxisID: "y"
                    },

                    {
                        label: "Iluminación",

                        data: light,

                        tension: 0.3,

                        pointRadius: 0,

                        pointHoverRadius: 5,

                        borderWidth: 2,

                        yAxisID: "yLight"
                    }

                ]

            },

            options: {

                responsive: true,

                maintainAspectRatio: false,

                interaction: {
                    mode: "index",
                    intersect: false
                },

                plugins: {

                    legend: {
                        position: "top",

                        labels: {
                            color: "#94a3b8",
                            usePointStyle: true,
                            boxWidth: 8
                        }
                    },

                    tooltip: {

                        callbacks: {

                            label: function(context) {

                                const value =
                                    context.parsed.y;

                                if (context.dataset.yAxisID === "yLight") {

                                    return `${context.dataset.label}: ${value} lux`;

                                }

                                return `${context.dataset.label}: ${value} %`;
                            }

                        }

                    }

                },

                scales: {

                    x: {

                        ticks: {
                            color: "#94a3b8",
                            maxTicksLimit: 8
                        },

                        grid: {
                            color: "rgba(148, 163, 184, 0.08)"
                        }

                    },

                    y: {

                        min: 0,
                        max: 100,

                        title: {
                            display: true,
                            text: "Humedad (%)",
                            color: "#94a3b8"
                        },

                        ticks: {
                            color: "#94a3b8"
                        },

                        grid: {
                            color: "rgba(148, 163, 184, 0.08)"
                        }

                    },

                    yLight: {

                        position: "right",

                        beginAtZero: true,

                        title: {
                            display: true,
                            text: "Iluminación (lux)",
                            color: "#94a3b8"
                        },

                        ticks: {
                            color: "#94a3b8"
                        },

                        grid: {
                            drawOnChartArea: false
                        }

                    }

                }

            }

        });

}

function formatTime(dateString) {

    const date =
        new Date(dateString);

    return date.toLocaleTimeString(
        "es-MX",
        {
            hour: "2-digit",
            minute: "2-digit"
        }
    );
}

document
    .getElementById("chart-period")
    .addEventListener("change", event => {

        const hours =
            Number(event.target.value);

        loadHistory(hours);

    });

refreshDashboard();
loadHistory(24);

setInterval(refreshDashboard, 5000);
setInterval(() => {
    loadHistory(24);
}, 10000);