using System;
using System.Collections.Generic;

namespace GestionRecetas.Models;

public partial class ProcesoGenerico
{
    public int ID { get; set; }

    public string? Reactor { get; set; }

    public string? Nombre { get; set; }

    public bool Bloqueada { get; set; }

    public DateTime? Creada { get; set; }

    public DateTime? Modificada { get; set; }

    public DateTime? Eliminada { get; set; }

}
