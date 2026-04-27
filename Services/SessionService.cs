using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PuntoVenta.Models;

namespace PuntoVenta.Services
{
    public static class SessionService
    {
        public static User CurrentUser { get; set; }
    }
}
