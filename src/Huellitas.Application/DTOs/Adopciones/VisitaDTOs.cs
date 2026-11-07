namespace Huellitas.Application.DTOs.Adopciones;

public class AgendarVisitaDTO
{
    public DateTime Fecha { get; set; }
    public byte Modalidad { get; set; } // 0 = Presencial, 1 = Virtual
}

public class ConfirmarVisitaDTO
{
    public bool Aceptar { get; set; } // true = Confirma, false = Rechaza la fecha
}