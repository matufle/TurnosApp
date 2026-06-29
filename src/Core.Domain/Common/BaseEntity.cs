using System;
using System.Collections.Generic;
using System.Text;

namespace TurnosApp.Core.Domain.Common;
//Esta clase es la base para todas las entidades del dominio,
//proporcionando una propiedad Id común para la identificación de cada entidad.
public abstract class BaseEntity
{
    public int Id { get; set; }
}