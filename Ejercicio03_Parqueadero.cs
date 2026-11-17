/*
Parqueadero de visitantes (versión 2).
Entradas : placa, torre, apartamento, hora (HH:MM, 24 h) y tipo (Carro/Moto/Bicicleta).
Proceso  : cada ingreso ocupa una fila de la matriz `datos` y de los vectores de horas (máx. 40).
           Una placa no puede ingresar otra vez hasta registrar su salida.
           Si la hora de salida es menor que la de ingreso, se pregunta si salió al día siguiente;
           la permanencia siempre debe ser menor a 24 horas.
Salidas  : tiempo de permanencia, ingresos por tipo (vector) y vehículo de mayor permanencia.
Uso      : dotnet run              -> menú interactivo
           dotnet run -- --prueba  -> pruebas automáticas
*/
using System;
using System.Text.RegularExpressions;

class Program
{
    const int MaxIngresos = 40;
    const int MinutosPorDia = 1440;
    const int SinSalida = -1;
    const int HoraInvalida = -1;
    const int ColPlaca = 0, ColTorre = 1, ColApto = 2, ColTipo = 3;

    static readonly string[] Tipos = { "Carro", "Moto", "Bicicleta" };
    static readonly Regex PatronHora = new Regex(@"\A([01]?[0-9]|2[0-3]):([0-5][0-9])\z");

    static string[,] datos = new string[MaxIngresos, 4]; // placa, torre, apto, tipo
    static int[] horaIngreso = new int[MaxIngresos];     // minutos desde las 00:00 del día de ingreso
    static int[] horaSalida = new int[MaxIngresos];      // SinSalida mientras siga dentro; puede ser >= 1440 si salió al día siguiente
    static int total = 0;
    static int fallos = 0;

    // ---------- Funciones ----------

    static string Normalizar(string texto)
    {
        return texto.Trim().ToUpperInvariant();
    }

    // Espera el texto ya normalizado (mayúsculas, sin espacios en los extremos).
    static bool EsAlfanumerico(string texto, int minimo, int maximo)
    {
        return Regex.IsMatch(texto, $@"\A[A-Z0-9]{{{minimo},{maximo}}}\z");
    }

    static bool PlacaValida(string placa) => EsAlfanumerico(placa, 3, 7);

    static bool CodigoValido(string codigo) => EsAlfanumerico(codigo, 1, 10);

    static int HoraAMinutos(string texto)
    {
        Match m = PatronHora.Match(texto.Trim());
        if (!m.Success) return HoraInvalida;
        return int.Parse(m.Groups[1].Value) * 60 + int.Parse(m.Groups[2].Value);
    }

    static string MinutosAHora(int minutos)
    {
        int enElDia = minutos % MinutosPorDia;
        string hora = $"{enElDia / 60:D2}:{enElDia % 60:D2}";
        return minutos >= MinutosPorDia ? hora + " (día siguiente)" : hora;
    }

    static string Duracion(int minutos)
    {
        return $"{minutos / 60} h {minutos % 60} min";
    }

    static int Permanencia(int fila)
    {
        return horaSalida[fila] - horaIngreso[fila];
    }

    // Fila de la placa que aún no sale; -1 si no está dentro.
    static int BuscarDentro(string placa)
    {
        for (int i = 0; i < total; i++)
            if (datos[i, ColPlaca] == placa && horaSalida[i] == SinSalida) return i;
        return -1;
    }

    static bool RegistrarIngreso(string placa, string torre, string apto, string hora, string tipo, out string mensaje)
    {
        placa = Normalizar(placa);
        torre = Normalizar(torre);
        apto = Normalizar(apto);

        if (total >= MaxIngresos)
        {
            mensaje = $"Se alcanzó el límite de {MaxIngresos} ingresos diarios.";
            return false;
        }
        if (!PlacaValida(placa))
        {
            mensaje = "Placa inválida (3 a 7 letras o números).";
            return false;
        }
        if (!CodigoValido(torre) || !CodigoValido(apto))
        {
            mensaje = "Torre y apartamento son obligatorios (letras o números).";
            return false;
        }
        int entrada = HoraAMinutos(hora);
        if (entrada == HoraInvalida)
        {
            mensaje = "Hora inválida. Use el formato HH:MM (00:00 a 23:59).";
            return false;
        }
        if (Array.IndexOf(Tipos, tipo) == -1)
        {
            mensaje = "Tipo de vehículo inválido.";
            return false;
        }
        if (BuscarDentro(placa) != -1)
        {
            mensaje = $"La placa {placa} ya está dentro y no ha registrado salida.";
            return false;
        }

        datos[total, ColPlaca] = placa;
        datos[total, ColTorre] = torre;
        datos[total, ColApto] = apto;
        datos[total, ColTipo] = tipo;
        horaIngreso[total] = entrada;
        horaSalida[total] = SinSalida;
        total++;
        mensaje = $"Ingreso registrado: {placa} a las {MinutosAHora(entrada)}.";
        return true;
    }

    // `diaSiguiente` indica que la hora de salida corresponde al día después del ingreso.
    static bool RegistrarSalida(string placa, string hora, bool diaSiguiente, out string mensaje)
    {
        placa = Normalizar(placa);
        int fila = BuscarDentro(placa);
        if (fila == -1)
        {
            mensaje = $"La placa {placa} no tiene un ingreso activo.";
            return false;
        }
        int salida = HoraAMinutos(hora);
        if (salida == HoraInvalida)
        {
            mensaje = "Hora inválida. Use el formato HH:MM (00:00 a 23:59).";
            return false;
        }
        if (diaSiguiente) salida += MinutosPorDia;
        if (salida < horaIngreso[fila])
        {
            mensaje = $"La salida no puede ser antes del ingreso ({MinutosAHora(horaIngreso[fila])}).";
            return false;
        }
        if (salida - horaIngreso[fila] >= MinutosPorDia)
        {
            mensaje = "La permanencia no puede ser de 24 horas o más.";
            return false;
        }
        horaSalida[fila] = salida;
        mensaje = $"Salida registrada. Permanencia: {Duracion(Permanencia(fila))}.";
        return true;
    }

    // Vector con la cantidad de ingresos de cada tipo de `Tipos`.
    static int[] ContarPorTipo()
    {
        int[] conteo = new int[Tipos.Length];
        for (int i = 0; i < total; i++)
            conteo[Array.IndexOf(Tipos, datos[i, ColTipo])]++;
        return conteo;
    }

    // Fila con mayor permanencia entre los que ya salieron; -1 si ninguno ha salido.
    static int IndiceMayorPermanencia()
    {
        int mejor = -1;
        int mejorMinutos = -1;
        for (int i = 0; i < total; i++)
        {
            if (horaSalida[i] != SinSalida && Permanencia(i) > mejorMinutos)
            {
                mejor = i;
                mejorMinutos = Permanencia(i);
            }
        }
        return mejor;
    }

    // ---------- Procedimientos ----------

    static string Leer(string mensaje)
    {
        Console.Write(mensaje);
        return Console.ReadLine() ?? "";
    }

    // Repite la pregunta hasta que la respuesta cumpla la validación.
    static string LeerValido(string mensaje, Func<string, bool> esValido, string error)
    {
        while (true)
        {
            string texto = Leer(mensaje);
            if (esValido(texto)) return texto;
            Console.WriteLine(error);
        }
    }

    static string LeerTipo()
    {
        for (int i = 0; i < Tipos.Length; i++)
            Console.Write($"{i + 1}={Tipos[i]}  ");
        Console.WriteLine();
        while (true)
        {
            string opcion = Leer("Tipo de vehículo: ").Trim();
            if (opcion.Length == 1 && opcion[0] >= '1' && opcion[0] < '1' + Tipos.Length)
                return Tipos[opcion[0] - '1'];
            Console.WriteLine("Opción inválida.");
        }
    }

    static void MostrarMenu()
    {
        Console.WriteLine("\n--- PARQUEADERO VISITANTES ---");
        Console.WriteLine("1. Registrar ingreso");
        Console.WriteLine("2. Registrar salida");
        Console.WriteLine("3. Consultar vehículo");
        Console.WriteLine("4. Ingresos por tipo");
        Console.WriteLine("5. Mayor permanencia");
        Console.WriteLine("6. Salir");
    }

    static void PedirIngreso()
    {
        if (total >= MaxIngresos)
        {
            Console.WriteLine($"Se alcanzó el límite de {MaxIngresos} ingresos diarios.");
            return;
        }
        string placa = LeerValido("Placa: ", t => PlacaValida(Normalizar(t)),
            "Placa inválida (3 a 7 letras o números).");
        string torre = LeerValido("Torre visitada: ", t => CodigoValido(Normalizar(t)),
            "La torre es obligatoria (letras o números).");
        string apto = LeerValido("Apartamento: ", t => CodigoValido(Normalizar(t)),
            "El apartamento es obligatorio (letras o números).");
        string hora = LeerValido("Hora de ingreso (HH:MM): ", t => HoraAMinutos(t) != HoraInvalida,
            "Hora inválida. Use el formato HH:MM (00:00 a 23:59).");
        string tipo = LeerTipo();

        string mensaje;
        RegistrarIngreso(placa, torre, apto, hora, tipo, out mensaje);
        Console.WriteLine(mensaje);
    }

    static void PedirSalida()
    {
        string placa = Leer("Placa: ");
        string hora = Leer("Hora de salida (HH:MM): ");

        // Solo se pregunta por el día siguiente cuando la hora es menor que la de ingreso.
        bool diaSiguiente = false;
        int fila = BuscarDentro(Normalizar(placa));
        int minutos = HoraAMinutos(hora);
        if (fila != -1 && minutos != HoraInvalida && minutos < horaIngreso[fila])
            diaSiguiente = Normalizar(Leer("La hora es menor que la de ingreso. ¿Salió al día siguiente? (S/N): ")) == "S";

        string mensaje;
        RegistrarSalida(placa, hora, diaSiguiente, out mensaje);
        Console.WriteLine(mensaje);
    }

    static void ConsultarVehiculo()
    {
        string placa = Normalizar(Leer("Placa a consultar: "));
        bool encontrado = false;
        for (int i = 0; i < total; i++)
        {
            if (datos[i, ColPlaca] != placa) continue;
            encontrado = true;
            string estado = horaSalida[i] == SinSalida
                ? "Aún dentro"
                : $"Salió {MinutosAHora(horaSalida[i])}, permanencia {Duracion(Permanencia(i))}";
            Console.WriteLine($"Torre {datos[i, ColTorre]} Apto {datos[i, ColApto]} | {datos[i, ColTipo]} | " +
                              $"Ingresó {MinutosAHora(horaIngreso[i])} | {estado}");
        }
        if (!encontrado)
            Console.WriteLine($"No hay registros para la placa {placa}.");
    }

    static void MostrarConteo()
    {
        int[] conteo = ContarPorTipo();
        for (int i = 0; i < Tipos.Length; i++)
            Console.WriteLine($"{Tipos[i]}: {conteo[i]}");
        Console.WriteLine($"Total: {total} de {MaxIngresos}");
    }

    static void MostrarMayorPermanencia()
    {
        int fila = IndiceMayorPermanencia();
        if (fila == -1)
        {
            Console.WriteLine("Aún no hay vehículos con salida registrada.");
            return;
        }
        Console.WriteLine($"Mayor permanencia: {datos[fila, ColPlaca]} ({datos[fila, ColTipo]}), " +
                          $"Torre {datos[fila, ColTorre]} Apto {datos[fila, ColApto]}, {Duracion(Permanencia(fila))}");
    }

    // ---------- Pruebas ----------

    static bool Ingresar(string placa, string torre, string apto, string hora, string tipo)
    {
        string mensaje;
        return RegistrarIngreso(placa, torre, apto, hora, tipo, out mensaje);
    }

    static bool Salir(string placa, string hora, bool diaSiguiente = false)
    {
        string mensaje;
        return RegistrarSalida(placa, hora, diaSiguiente, out mensaje);
    }

    static bool LlenarHastaElLimite()
    {
        for (int i = 0; total < MaxIngresos; i++)
            Ingresar("TST" + i.ToString("D3"), "3", "301", "12:00", "Moto");
        return total == MaxIngresos;
    }

    // Los casos comparten estado (el orden importa).
    static (string Caso, Func<bool> Prueba)[] CasosDePrueba()
    {
        return new (string, Func<bool>)[]
        {
            ("Ingreso válido (carro)",        () => Ingresar("abc123", "1", "101", "08:00", "Carro")),
            ("Placa repetida sin salida",     () => !Ingresar("ABC123", "1", "101", "08:30", "Carro")),
            ("Ingreso válido (moto)",         () => Ingresar("XYZ12D", "2", "202", "09:15", "Moto")),
            ("Ingreso válido (bici)",         () => Ingresar("BIC001", "1", "303", "10:00", "Bicicleta")),
            ("Placa inválida",                () => !Ingresar("A!", "1", "101", "08:00", "Carro")),
            ("Hora inválida 25:00",           () => !Ingresar("QWE456", "1", "101", "25:00", "Carro")),
            ("Hora inválida 8:5",             () => !Ingresar("QWE456", "1", "101", "8:5", "Carro")),
            ("Tipo inválido",                 () => !Ingresar("QWE456", "1", "101", "08:00", "Camión")),
            ("Torre vacía",                   () => !Ingresar("QWE456", "", "101", "08:00", "Carro")),
            ("Salida antes del ingreso",      () => !Salir("ABC123", "07:00")),
            ("Salida de placa inexistente",   () => !Salir("ZZZ999", "10:00")),
            ("Mayor permanencia sin salidas", () => IndiceMayorPermanencia() == -1),
            ("Salida ABC123",                 () => Salir("ABC123", "10:30")),
            ("Permanencia de 2 h 30 min",     () => Duracion(Permanencia(0)) == "2 h 30 min"),
            ("Salida XYZ12D",                 () => Salir("XYZ12D", "09:45")),
            ("Reingreso tras salir",          () => Ingresar("ABC123", "1", "101", "11:00", "Carro")),
            ("Conteo por tipo (2,1,1)",       () => string.Join(",", ContarPorTipo()) == "2,1,1"),
            ("Mayor permanencia en fila 0",   () => IndiceMayorPermanencia() == 0),

            // Cruce de medianoche
            ("Ingreso nocturno 23:00",        () => Ingresar("NOC001", "4", "401", "23:00", "Moto")),
            ("01:00 sin día siguiente falla", () => !Salir("NOC001", "01:00")),
            ("Salida 01:00 día siguiente",    () => Salir("NOC001", "01:00", true)),
            ("Permanencia nocturna 2 h 0 min",() => Duracion(Permanencia(4)) == "2 h 0 min"),
            ("Ingreso 08:00 (NOC002)",        () => Ingresar("NOC002", "4", "402", "08:00", "Carro")),
            ("Permanencia de 24 h rechazada", () => !Salir("NOC002", "08:00", true)),
            ("Permanencia de 23 h aceptada",  () => Salir("NOC002", "07:00", true)),
            ("Mayor permanencia cruzada",     () => IndiceMayorPermanencia() == 5),
            ("Se llegó al límite de 40",      LlenarHastaElLimite),
            ("Ingreso 41 rechazado",          () => !Ingresar("LIM001", "3", "301", "12:00", "Moto")),
        };
    }

    static void Probar()
    {
        foreach (var (caso, prueba) in CasosDePrueba())
        {
            bool cumple = prueba();
            Console.WriteLine($"[{(cumple ? "OK" : "FALLO")}] {caso}");
            if (!cumple) fallos++;
        }
        Console.WriteLine(fallos == 0 ? "RESULTADO: todas las pruebas pasaron" : $"RESULTADO: hay {fallos} fallos");
    }

    static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        if (args.Length > 0 && args[0] == "--prueba")
        {
            Probar();
            return fallos == 0 ? 0 : 1;
        }

        string opcion = "";
        while (opcion != "6")
        {
            MostrarMenu();
            Console.Write("Opción: ");
            string linea = Console.ReadLine();
            if (linea == null)
            {
                Console.WriteLine("\nFin del programa.");
                break;
            }
            opcion = linea.Trim();

            if (opcion == "1") PedirIngreso();
            else if (opcion == "2") PedirSalida();
            else if (opcion == "3") ConsultarVehiculo();
            else if (opcion == "4") MostrarConteo();
            else if (opcion == "5") MostrarMayorPermanencia();
            else if (opcion != "6") Console.WriteLine("Opción inválida.");
        }
        return 0;
    }
}
