using System.ComponentModel.DataAnnotations;

namespace appWeb2.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string nombre { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido")]
        public string correo { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string contrasena { get; set; }

        [Required(ErrorMessage = "Debes confirmar tu contraseña")]
        [Compare("contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string verificarContrasena { get; set; }
    }
}