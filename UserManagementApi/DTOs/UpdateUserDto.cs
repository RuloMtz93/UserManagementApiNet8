using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.DTOs;

public class UpdateUserDto
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "El correo electrónico no puede superar los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Range(0, 1, ErrorMessage = "El status debe ser 1 (Activo) o 0 (Inactivo).")]
    public byte Status { get; set; }
}