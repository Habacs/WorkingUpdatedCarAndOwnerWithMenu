using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Csabababa
{
    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
        public List<Car> Cars { get; set; } = new List<Car>();
    }

    public class Car
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public Owner Owner { get; set; }
    }

    public class CarDbContext : DbContext
    {
        public DbSet<Owner> Owners { get; set; }
        public DbSet<Car> Cars { get; set; }

        public CarDbContext(DbContextOptions<CarDbContext> options) : base(options) { }

        public CarDbContext() { }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Owner>()
                .ToTable("owners")
                .HasKey(o => o.Id);

            modelBuilder.Entity<Owner>()
                .Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Owner>()
                .Property(o => o.Age)
                .IsRequired();

            modelBuilder.Entity<Owner>()
                .Property(o => o.Address)
                .HasMaxLength(255);

            modelBuilder.Entity<Car>()
                .ToTable("cars")
                .HasKey(c => c.Id);

            modelBuilder.Entity<Car>()
                .Property(c => c.Model)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Car>()
                .Property(c => c.Year)
                .IsRequired();

            modelBuilder.Entity<Car>()
                .HasOne(c => c.Owner)
                .WithMany(o => o.Cars)
                .HasForeignKey(c => c.OwnerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CarDbContext>();
            optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=OwnerAndCarDb;Integrated Security=True;");

            using (var context = new CarDbContext(optionsBuilder.Options))
            {
                context.Database.EnsureCreated();

                while (true)
                {
                    int selectedOption = ShowMenu();
                    Console.Clear();

                    switch (selectedOption)
                    {
                        case 0:
                            AddOwner(context);
                            break;
                        case 1:
                            AddCar(context);
                            break;
                        case 2:
                            ShowDatabase(context);
                            break;
                        case 3:
                            DeleteOwner(context);
                            break;
                        case 4:
                            DeleteCar(context);
                            break;
                        case 5:
                            return;
                        default:
                            Console.WriteLine("Invalid choice, try again.");
                            break;
                    }
                }
            }
        }

        static int ShowMenu()
        {
            string[] options = { "Add Owner", "Add Car", "Show Database", "Delete Owner", "Delete Car", "Exit" };
            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                for (int i = 0; i < options.Length; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green; 
                        Console.WriteLine($"> {options[i]}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {options[i]}");
                    }
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex == 0) ? options.Length - 1 : selectedIndex - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex == options.Length - 1) ? 0 : selectedIndex + 1;
                }

            } while (key != ConsoleKey.Enter);

            return selectedIndex;
        }

        static void AddOwner(CarDbContext context)
        {
            Console.WriteLine("Enter owner details:");

            Console.Write("Name: ");
            string name = Console.ReadLine();

            Console.Write("Age: ");
            int age = int.Parse(Console.ReadLine());

            Console.Write("Address: ");
            string address = Console.ReadLine();

            var owner = new Owner
            {
                Name = name,
                Age = age,
                Address = address
            };

            context.Owners.Add(owner);
            context.SaveChanges();

            Console.WriteLine("Owner added successfully!");
            Console.ReadKey();
        }

        static void AddCar(CarDbContext context)
        {
            var owners = context.Owners.ToList();
            if (!owners.Any())
            {
                Console.WriteLine("No owners found. Please add an owner first.");
                Console.ReadKey();
                return;
            }

            int selectedOwnerIndex = ShowOwnerSelectionMenu(owners);

            var selectedOwner = owners[selectedOwnerIndex];

            Console.Write("Enter car model: ");
            string model = Console.ReadLine();

            Console.Write("Enter car year: ");
            int year = int.Parse(Console.ReadLine());

            var car = new Car
            {
                OwnerId = selectedOwner.Id,
                Model = model,
                Year = year
            };

            context.Cars.Add(car);
            context.SaveChanges();

            Console.WriteLine("Car added successfully!");
            Console.ReadKey();
        }

        static void ShowDatabase(CarDbContext context)
        {
            var owners = context.Owners.Include(o => o.Cars).ToList();

            if (!owners.Any())
            {
                Console.WriteLine("No owners found.");
                Console.ReadKey();
                return;
            }

            foreach (var owner in owners)
            {
                Console.WriteLine($"Owner: {owner.Name}, Age: {owner.Age}, Address: {owner.Address}");
                Console.WriteLine("Cars:");
                foreach (var car in owner.Cars)
                {
                    Console.WriteLine($"  Model: {car.Model}, Year: {car.Year}");
                }
            }

            Console.ReadKey();
        }

        static void DeleteOwner(CarDbContext context)
        {
            var owners = context.Owners.ToList();
            if (!owners.Any())
            {
                Console.WriteLine("No owners found to delete.");
                Console.ReadKey();
                return;
            }

            int selectedOwnerIndex = ShowOwnerSelectionMenu(owners);

            var selectedOwner = owners[selectedOwnerIndex];

            context.Owners.Remove(selectedOwner);
            context.SaveChanges();

            Console.WriteLine("Owner and their cars deleted successfully.");
            Console.ReadKey();
        }

        static void DeleteCar(CarDbContext context)
        {
            var owners = context.Owners.Include(o => o.Cars).ToList();
            if (!owners.Any())
            {
                Console.WriteLine("No owners found.");
                Console.ReadKey();
                return;
            }

            int selectedOwnerIndex = ShowOwnerSelectionMenu(owners);

            var selectedOwner = owners[selectedOwnerIndex];

            if (!selectedOwner.Cars.Any())
            {
                Console.WriteLine("This owner has no cars to delete.");
                Console.ReadKey();
                return;
            }

            int selectedCarIndex = ShowCarSelectionMenu(selectedOwner.Cars);

            var selectedCar = selectedOwner.Cars[selectedCarIndex];

            context.Cars.Remove(selectedCar);
            context.SaveChanges();

            Console.WriteLine("Car deleted successfully.");
            Console.ReadKey();
        }

        static int ShowOwnerSelectionMenu(System.Collections.Generic.List<Owner> owners)
        {
            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine("Select an owner:");

                for (int i = 0; i < owners.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green; 
                        Console.WriteLine($"> {owners[i].Name}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  {owners[i].Name}");
                    }
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex == 0) ? owners.Count - 1 : selectedIndex - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex == owners.Count - 1) ? 0 : selectedIndex + 1;
                }

            } while (key != ConsoleKey.Enter);

            return selectedIndex;
        }

        static int ShowCarSelectionMenu(System.Collections.Generic.List<Car> cars)
        {
            int selectedIndex = 0;
            ConsoleKey key;

            do
            {
                Console.Clear();
                Console.WriteLine("Select a car to delete:");

                for (int i = 0; i < cars.Count; i++)
                {
                    if (i == selectedIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green; 
                        Console.WriteLine($"> Model: {cars[i].Model}, Year: {cars[i].Year}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine($"  Model: {cars[i].Model}, Year: {cars[i].Year}");
                    }
                }

                key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex == 0) ? cars.Count - 1 : selectedIndex - 1;
                }
                else if (key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex == cars.Count - 1) ? 0 : selectedIndex + 1;
                }

            } while (key != ConsoleKey.Enter);

            return selectedIndex;
        }
    }
}
