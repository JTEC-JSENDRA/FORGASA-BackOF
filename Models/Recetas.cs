using System;
using System.Collections.Generic;

namespace GestionRecetas.Models;

public partial class Recetas
{
    public long ID { get; set; }

    public string? ID_Reactor { get; set; }

    public string? Nombre { get; set; }

    public short Bloqueada { get; set; }

    public DateTime? Creada { get; set; }

    public DateTime? Modificada { get; set; }

    public DateTime? Eliminada { get; set; }

}
