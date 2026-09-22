using Microsoft.EntityFrameworkCore;
using PlantControlWeb.Data;
using PlantControlWeb.Models;

namespace PlantControlWeb.Services;

public class EnvironmentEvaluationService
{
    private readonly PlantControlDbContext _context;

    public EnvironmentEvaluationService(PlantControlDbContext context)
    {
        _context = context;
    }

    public async Task EvaluateAsync(
        Device device,
        EnvironmentMeasurement measurement)
    {
        var profile = await _context.GrowingProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == device.ProfileId);

        if (profile == null)
            return;

        await EvaluateTemperatureAir(device, measurement, profile);
        await EvaluateTemperatureSoil(device, measurement, profile);
        await EvaluateHumidity(device, measurement, profile);
        await EvaluateSoilMoisture(device, measurement, profile);
        await EvaluateLight(device, measurement, profile);

        // Guardamos todos los cambios de esta evaluación
        // en una sola operación.
        await _context.SaveChangesAsync();
    }

    private async Task EvaluateTemperatureAir(
        Device device,
        EnvironmentMeasurement measurement,
        GrowingProfile profile)
    {
        if (!measurement.TemperatureAir.HasValue)
            return;

        if (profile.TemperatureAirMin.HasValue &&
            measurement.TemperatureAir < profile.TemperatureAirMin)
        {
            await EvaluateCondition(
                device,
                measurement,
                "temperature_air_low",
                "warning",
                $"Temperatura del aire demasiado baja: {measurement.TemperatureAir} °C.");
        }
        else if (profile.TemperatureAirMax.HasValue &&
                 measurement.TemperatureAir > profile.TemperatureAirMax)
        {
            await EvaluateCondition(
                device,
                measurement,
                "temperature_air_high",
                "warning",
                $"Temperatura del aire demasiado alta: {measurement.TemperatureAir} °C.");
        }
        else
        {
            await ResolveAlert(device.Id, "temperature_air_low");
            await ResolveAlert(device.Id, "temperature_air_high");
        }
    }

    private async Task EvaluateTemperatureSoil(
        Device device,
        EnvironmentMeasurement measurement,
        GrowingProfile profile)
    {
        if (!measurement.TemperatureSoil.HasValue)
            return;

        if (profile.TemperatureSoilMin.HasValue &&
            measurement.TemperatureSoil < profile.TemperatureSoilMin)
        {
            await EvaluateCondition(
                device,
                measurement,
                "temperature_soil_low",
                "warning",
                $"Temperatura del sustrato demasiado baja: {measurement.TemperatureSoil} °C.");
        }
        else if (profile.TemperatureSoilMax.HasValue &&
                 measurement.TemperatureSoil > profile.TemperatureSoilMax)
        {
            await EvaluateCondition(
                device,
                measurement,
                "temperature_soil_high",
                "warning",
                $"Temperatura del sustrato demasiado alta: {measurement.TemperatureSoil} °C.");
        }
        else
        {
            await ResolveAlert(device.Id, "temperature_soil_low");
            await ResolveAlert(device.Id, "temperature_soil_high");
        }
    }

    private async Task EvaluateHumidity(
        Device device,
        EnvironmentMeasurement measurement,
        GrowingProfile profile)
    {
        if (!measurement.Humidity.HasValue)
            return;

        if (profile.HumidityMin.HasValue &&
            measurement.Humidity < profile.HumidityMin)
        {
            await EvaluateCondition(
                device,
                measurement,
                "humidity_low",
                "warning",
                $"Humedad ambiental demasiado baja: {measurement.Humidity}%.");
        }
        else if (profile.HumidityMax.HasValue &&
                 measurement.Humidity > profile.HumidityMax)
        {
            await EvaluateCondition(
                device,
                measurement,
                "humidity_high",
                "warning",
                $"Humedad ambiental demasiado alta: {measurement.Humidity}%.");
        }
        else
        {
            await ResolveAlert(device.Id, "humidity_low");
            await ResolveAlert(device.Id, "humidity_high");
        }
    }

    private async Task EvaluateSoilMoisture(
        Device device,
        EnvironmentMeasurement measurement,
        GrowingProfile profile)
    {
        if (!measurement.SoilMoisture.HasValue)
            return;

        if (profile.SoilMoistureMin.HasValue &&
            measurement.SoilMoisture < profile.SoilMoistureMin)
        {
            await EvaluateCondition(
                device,
                measurement,
                "soil_moisture_low",
                "warning",
                $"Humedad del sustrato demasiado baja: {measurement.SoilMoisture}%.");
        }
        else if (profile.SoilMoistureMax.HasValue &&
                 measurement.SoilMoisture > profile.SoilMoistureMax)
        {
            await EvaluateCondition(
                device,
                measurement,
                "soil_moisture_high",
                "warning",
                $"Humedad del sustrato demasiado alta: {measurement.SoilMoisture}%.");
        }
        else
        {
            await ResolveAlert(device.Id, "soil_moisture_low");
            await ResolveAlert(device.Id, "soil_moisture_high");
        }
    }

    private async Task EvaluateLight(
        Device device,
        EnvironmentMeasurement measurement,
        GrowingProfile profile)
    {
        if (!measurement.Light.HasValue)
            return;

        if (profile.LightMin.HasValue &&
            measurement.Light < profile.LightMin)
        {
            await EvaluateCondition(
                device,
                measurement,
                "light_low",
                "warning",
                $"Nivel de luz demasiado bajo: {measurement.Light}.");
        }
        else if (profile.LightMax.HasValue &&
                 measurement.Light > profile.LightMax)
        {
            await EvaluateCondition(
                device,
                measurement,
                "light_high",
                "warning",
                $"Nivel de luz demasiado alto: {measurement.Light}.");
        }
        else
        {
            await ResolveAlert(device.Id, "light_low");
            await ResolveAlert(device.Id, "light_high");
        }
    }

    private async Task EvaluateCondition(
        Device device,
        EnvironmentMeasurement measurement,
        string type,
        string severity,
        string message)
    {
        var activeAlert = await _context.Alerts
            .FirstOrDefaultAsync(a =>
                a.DeviceId == device.Id &&
                a.Type == type &&
                a.ResolvedAt == null);

        // Si ya existe una alerta activa del mismo tipo,
        // no creamos otra.
        if (activeAlert != null)
            return;

        var alert = new Alert
        {
            DeviceId = device.Id,
            MeasurementId = measurement.Id,
            Type = type,
            Severity = severity,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        _context.Alerts.Add(alert);
    }

    private async Task ResolveAlert(
        long deviceId,
        string type)
    {
        var activeAlert = await _context.Alerts
            .FirstOrDefaultAsync(a =>
                a.DeviceId == deviceId &&
                a.Type == type &&
                a.ResolvedAt == null);

        if (activeAlert == null)
            return;

        activeAlert.ResolvedAt = DateTime.UtcNow;
    }
}