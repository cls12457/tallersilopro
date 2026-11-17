// Sistema de control de inventario para una cafetería universitaria
using System;

class Program
{
    class Producto
    {
        public string Cod { get; set; }
        public string Nom { get; set; }
        public double Pre { get; set; }
        public int Stk { get; set; }
        public string Cat { get; set; }
    }

    static void Main(string[] args)
    {
        Producto[] inv = new Producto[20];
        int total = 0;
        double totalDia = 0;
        int op;

        do
        {
            Console.Clear();
            Console.WriteLine("=== CONTROL DE INVENTARIO CAFETERIA U ===");
            Console.WriteLine("1. Registrar producto");
            Console.WriteLine("2. Realizar venta");
            Console.WriteLine("3. Actualizar inventario");
            Console.WriteLine("4. Mostrar productos agotados");
            Console.WriteLine("5. Calcular total vendido del día");
            Console.WriteLine("6. Salir");
            Console.Write("Seleccione opción: ");

            if (!int.TryParse(Console.ReadLine(), out op))
            {
                Console.WriteLine("\n[ERROR] Ingrese un número válido.");
                Console.ReadKey();
                continue;
            }

            if (op == 1)
            {
                if (total >= 20)
                {
                    Console.WriteLine("\nInventario lleno (máximo 20 productos).");
                }
                else
                {
                    var p = new Producto();
                    Console.Write("Código: "); p.Cod = Console.ReadLine();
                    Console.Write("Nombre: "); p.Nom = Console.ReadLine();

                    // Validación con variable temporal para evitar error out/ref
                    double tempPre;
                    do
                    {
                        Console.Write("Precio unitario (>0): ");
                    } while (!double.TryParse(Console.ReadLine(), out tempPre) || tempPre <= 0);
                    p.Pre = tempPre;

                    // Validación con variable temporal
                    int tempStk;
                    do
                    {
                        Console.Write("Stock disponible (>=0): ");
                    } while (!int.TryParse(Console.ReadLine(), out tempStk) || tempStk < 0);
                    p.Stk = tempStk;

                    Console.Write("Categoría (1: Bebidas, 2: Panadería, 3: Snacks): ");
                    string c = Console.ReadLine();
                    p.Cat = c == "1" ? "Bebidas" : (c == "2" ? "Panadería" : "Snacks");

                    inv[total++] = p;
                    Console.WriteLine("\n¡Producto registrado con éxito!");
                }
            }
            else if (op == 2 || op == 3)
            {
                if (total == 0)
                {
                    Console.WriteLine("\nNo hay productos registrados.");
                }
                else
                {
                    Console.Write("Ingrese el código del producto: ");
                    string cod = Console.ReadLine();
                    int idx = -1;
                    for (int i = 0; i < total; i++)
                    {
                        if (inv[i].Cod.Equals(cod, StringComparison.OrdinalIgnoreCase)) idx = i;
                    }

                    if (idx != -1)
                    {
                        var p = inv[idx];
                        Console.WriteLine($"Producto: {p.Nom} | Stock actual: {p.Stk} | Precio: ${p.Pre}");
                        Console.Write(op == 2 ? "Cantidad a vender: " : "Cantidad a agregar al stock: ");

                        if (int.TryParse(Console.ReadLine(), out int cant) && cant > 0)
                        {
                            if (op == 2)
                            {
                                if (cant <= p.Stk)
                                {
                                    p.Stk -= cant;
                                    totalDia += cant * p.Pre;
                                    Console.WriteLine($"\n¡Venta exitosa! Subtotal: ${cant * p.Pre:F2}");
                                }
                                else
                                {
                                    Console.WriteLine("\n[ERROR] Stock insuficiente.");
                                }
                            }
                            else
                            {
                                p.Stk += cant;
                                Console.WriteLine($"\n¡Stock actualizado! Nuevo stock: {p.Stk}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\n[ERROR] Cantidad inválida.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("\n[ERROR] Producto no encontrado.");
                    }
                }
            }
            else if (op == 4)
            {
                Console.WriteLine("\n--- PRODUCTOS AGOTADOS ---");
                bool hay = false;
                for (int i = 0; i < total; i++)
                {
                    if (inv[i].Stk == 0)
                    {
                        Console.WriteLine($"- [{inv[i].Cod}] {inv[i].Nom} ({inv[i].Cat})");
                        hay = true;
                    }
                }
                if (!hay) Console.WriteLine("No hay productos agotados.");
            }
            else if (op == 5)
            {
                Console.WriteLine("\n--- REPORTE FINANCIERO ---");
                Console.WriteLine($"Total vendido en el día: ${totalDia:F2}");
            }

            if (op != 6)
            {
                Console.WriteLine("\nPresione una tecla para continuar...");
                Console.ReadKey();
            }
        } while (op != 6);
    }
}
