using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Mascotas;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Interfaces.Repositories;

namespace Huellitas.Application.Services;

public class MatchService : IMatchService
{
    private readonly IMascotaRepository _mascotaRepo;
    private readonly IPerfilAdoptanteRepository _perfilRepo;

    public MatchService(IMascotaRepository mascotaRepo, IPerfilAdoptanteRepository perfilRepo)
    {
        _mascotaRepo = mascotaRepo;
        _perfilRepo = perfilRepo;
    }

    public async Task<IEnumerable<RecomendacionMascotaDTO>> ObtenerRecomendacionesAsync(int usuarioId)
    {
        //Validar que el perfil exista y esté completo
        var perfil = await _perfilRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Debes crear tu perfil de adoptante para ver recomendaciones.");

        if (!perfil.TipoVivienda.HasValue || !perfil.PresenciaDeNinos.HasValue || !perfil.OtrasMascotas.HasValue)
            throw AppException.BadRequest("Debes completar tu perfil de adoptante (vivienda, niños, otras mascotas, etc.) para ver recomendaciones.");

        //Traer SOLO una muestra aleatoria de 50 mascotas (Optimización de rendimiento)
        var mascotas = await _mascotaRepo.ObtenerMuestraMascotasDisponiblesAsync(50);

        var recomendaciones = new List<RecomendacionMascotaDTO>();

        //Calcular el puntaje para cada mascota de la muestra
        foreach (var mascota in mascotas)
        {
            int puntaje = 0;
            var factores = new List<string>();

            // --- REGLA 1: Espacio y Tamaño (Max 30 pts) ---
            var tamanoNombre = mascota.Tamano?.Nombre.ToLower() ?? "";
            if (perfil.TipoVivienda == 1) // 1 = Apartamento
            {
                if (tamanoNombre == "pequeño") { puntaje += 30; factores.Add("Ideal para apartamento por su tamaño."); }
                else if (tamanoNombre == "mediano") { puntaje += 15; factores.Add("Tamaño aceptable para apartamento."); }
                else { puntaje += 0; factores.Add("Su tamaño puede ser complicado en un apartamento."); }
            }
            else // Casa o Finca
            {
                puntaje += 30;
                factores.Add("Tiene espacio suficiente para su tamaño.");
            }

            // --- REGLA 2: Niños (Max 20 pts) ---
            var temperamentos = mascota.Temperamentos.Select(t => t.Nombre.ToLower()).ToList();
            bool esSociable = temperamentos.Contains("juguetón") || temperamentos.Contains("cariñoso");

            if (perfil.PresenciaDeNinos == true)
            {
                if (esSociable) { puntaje += 20; factores.Add("Tiene un temperamento ideal para niños."); }
                else { puntaje += 0; factores.Add("Puede necesitar un hogar sin niños pequeños."); }
            }
            else { puntaje += 20; }

            // --- REGLA 3: Otras Mascotas (Max 20 pts) ---
            bool esTolerante = temperamentos.Contains("tranquilo") || temperamentos.Contains("independiente");
            if (perfil.OtrasMascotas == true)
            {
                if (esTolerante) { puntaje += 20; factores.Add("Se lleva bien con otras mascotas."); }
                else { puntaje += 0; factores.Add("Podría ser territorial con otras mascotas."); }
            }
            else { puntaje += 20; }

            // --- REGLA 4: Tiempo (Max 20 pts) ---
            if (perfil.TiempoDisponible != null && perfil.TiempoDisponible.ToLower().Contains("poco"))
            {
                if (esTolerante) { puntaje += 20; factores.Add("Se adapta a tu tiempo disponible."); }
                else { puntaje += 0; factores.Add("Requiere más tiempo del que dispones actualmente."); }
            }
            else
            {
                if (temperamentos.Contains("juguetón")) { puntaje += 20; factores.Add("Aprovechará el tiempo que tienes para jugar."); }
                else { puntaje += 10; }
            }

            // --- REGLA 5: Experiencia (Max 10 pts) ---
            if (perfil.ExperienciaPrevia == false)
            {
                if (esTolerante || esSociable) { puntaje += 10; factores.Add("Ideal para una primera adopción."); }
            }
            else { puntaje += 10; }

            //Mapear al DTO final
            var dto = new RecomendacionMascotaDTO
            {
                Puntaje = puntaje,
                Factores = factores,
                Id = mascota.MascotaId,
                Nombre = mascota.Nombre,
                Especie = mascota.Especie?.Nombre ?? "",
                Raza = mascota.Raza?.Nombre,
                Sexo = mascota.Sexo == 0 ? "Macho" : "Hembra",
                EdadAproximada = mascota.EdadAproximada ?? 0,
                Estado = mascota.Estado == 0 ? "Disponible" : "En Proceso",
                FotoPrincipal = mascota.Multimedia?.FirstOrDefault(m => m.EsPrincipal)?.Archivo,
                FechaCreacion = mascota.FechaCreacion
            };

            recomendaciones.Add(dto);
        }

        //Ordenar por puntaje y luego usar un Guid aleatorio para desempatar
        return recomendaciones
            .OrderByDescending(r => r.Puntaje)
            .ThenBy(r => Guid.NewGuid())
            .Take(10);
    }
}