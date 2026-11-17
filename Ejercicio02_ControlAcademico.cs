// Sistema de control académico para seguimiento de notas
using System;

class Program
{
    class Estudiante
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public double[] Notas { get; set; } = new double[3];
        public double Asistencia { get; set; }
        public double Definitiva { get; private set; }
        public string Estado { get; private set; }

        public void Calcular()
        {
            Definitiva = (Notas[0] + Notas[1] + Notas[2]) / 3.0;
            Estado = Definitiva >= 3.0 ? "Aprobado" : (Definitiva >= 2.0 ? "Habilita" : "Reprueba");
        }
    }

    static void Main(string[] args)
    {
        Estudiante[] ests = new Estudiante[25];
        int total = 0, opcion;

        do
        {
            Console.Clear();
            Console.WriteLine("=== CONTROL ACADÉMICO PARA SEGUIMIENTO DE NOTAS ===");
            Console.WriteLine("1. Registrar estudiante");
            Console.WriteLine("2. Ver reporte general y estadísticas");
            Console.WriteLine("3. Salir");
            Console.Write("Seleccione opción: ");

            if (!int.TryParse(Console.ReadLine(), out opcion)) continue;

            if (opcion == 1 && total < 25)
            {
                Estudiante e = new Estudiante();
                Console.Write("Código: "); e.Codigo = Console.ReadLine();
                Console.Write("Nombre: "); e.Nombre = Console.ReadLine();

                // Validación corregida para las 3 notas
                for (int i = 0; i < 3; i++)
                {
                    double notaTemp;
                    bool notaValida = false;
                    do
                    {
                        Console.Write($"Nota {i + 1} (0.0 - 5.0): ");
                        // Usamos una variable temporal para evitar el error de out/ref
                        if (double.TryParse(Console.ReadLine(), out notaTemp) && notaTemp >= 0 && notaTemp <= 5)
                        {
                            e.Notas[i] = notaTemp;
                            notaValida = true;
                        }
                        else
                        {
                            Console.WriteLine("Error: Ingrese un valor válido entre 0.0 y 5.0.");
                        }
                    } while (!notaValida);
                }

                // Validación de asistencia corregida con variable temporal
                double asistTemp;
                bool asistValida = false;
                do
                {
                    Console.Write("Asistencia % (0 - 100): ");
                    if (double.TryParse(Console.ReadLine(), out asistTemp) && asistTemp >= 0 && asistTemp <= 100)
                    {
                        e.Asistencia = asistTemp;
                        asistValida = true;
                    }
                    else
                    {
                        Console.WriteLine("Error: Ingrese un porcentaje válido entre 0 y 100.");
                    }
                } while (!asistValida);

                e.Calcular();
                ests[total++] = e;
                Console.WriteLine("\n¡Registrado con éxito! Presione una tecla...");
                Console.ReadKey();
            }
            else if (opcion == 2 && total > 0)
            {
                double suma = 0, mejor = ests[0].Definitiva, peor = ests[0].Definitiva;
                int m = 0, p = 0;

                Console.WriteLine("\n--- REPORTE ACADÉMICO ---");
                for (int i = 0; i < total; i++)
                {
                    var e = ests[i];
                    Console.WriteLine($"[{e.Codigo}] {e.Nombre} | Def: {e.Definitiva:F2} | {e.Estado}");
                    suma += e.Definitiva;
                    if (e.Definitiva > mejor) { mejor = e.Definitiva; m = i; }
                    if (e.Definitiva < peor) { peor = e.Definitiva; p = i; }
                }

                Console.WriteLine($"\nPromedio Grupo: {suma / total:F2}");
                Console.WriteLine($"Mejor Promedio: {ests[m].Nombre} ({mejor:F2})");
                Console.WriteLine($"Peor Promedio: {ests[p].Nombre} ({peor:F2})");
                Console.WriteLine("\nPresione una tecla para continuar...");
                Console.ReadKey();
            }
        } while (opcion != 3);
    }
}
