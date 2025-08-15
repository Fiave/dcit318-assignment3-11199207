using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthcareSystem
{
    public class Repository<T>
    {
        private readonly List<T> items = new List<T>();

        public void Add(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            items.Add(item);
        }

        public List<T> GetAll()
        {
            return new List<T>(items);
        }

        public T GetById(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            return items.FirstOrDefault(predicate);
        }

        public bool Remove(Func<T, bool> predicate)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var match = items.FirstOrDefault(predicate);
            if (object.Equals(match, default(T))) return false;
            return items.Remove(match);
        }
    }

    // ---------- Domain Models ----------
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }

        public Patient(int id, string name, int age, string gender)
        {
            Id = id; Name = name; Age = age; Gender = gender;
        }

        public override string ToString()
        {
            return $"ID={Id}, Name={Name}, Age={Age}, Gender={Gender}";
        }
    }

    public class Prescription
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string MedicationName { get; set; }
        public DateTime DateIssued { get; set; }

        public Prescription(int id, int patientId, string medicationName, DateTime dateIssued)
        {
            Id = id; PatientId = patientId; MedicationName = medicationName; DateIssued = dateIssued;
        }

        public override string ToString()
        {
            return $"RxID={Id}, PatientId={PatientId}, Medication={MedicationName}, Date={DateIssued:yyyy-MM-dd}";
        }
    }

    // Application Layer
    public class HealthSystemApp
    {
        private readonly Repository<Patient> _patientRepo = new Repository<Patient>();
        private readonly Repository<Prescription> _prescriptionRepo = new Repository<Prescription>();
        private readonly Dictionary<int, List<Prescription>> _prescriptionMap = new Dictionary<int, List<Prescription>>();

        // Expose repositories and map only if needed (kept private per spec)

        public void SeedData()
        {
            // Patients (2–3)
            _patientRepo.Add(new Patient(1, "Alice Mensah", 29, "F"));
            _patientRepo.Add(new Patient(2, "Kwame Boateng", 41, "M"));
            _patientRepo.Add(new Patient(3, "Esi Owusu", 35, "F"));

            // Prescriptions (4–5) with valid PatientIds
            _prescriptionRepo.Add(new Prescription(101, 1, "Amoxicillin 500mg", DateTime.Today.AddDays(-10)));
            _prescriptionRepo.Add(new Prescription(102, 1, "Ibuprofen 200mg", DateTime.Today.AddDays(-5)));
            _prescriptionRepo.Add(new Prescription(103, 2, "Metformin 500mg", DateTime.Today.AddDays(-2)));
            _prescriptionRepo.Add(new Prescription(104, 3, "Vitamin D 1000IU", DateTime.Today.AddDays(-1)));
            _prescriptionRepo.Add(new Prescription(105, 2, "Lisinopril 10mg", DateTime.Today));
        }

        public void BuildPrescriptionMap()
        {
            _prescriptionMap.Clear();
            foreach (var rx in _prescriptionRepo.GetAll())
            {
                if (!_prescriptionMap.ContainsKey(rx.PatientId))
                    _prescriptionMap[rx.PatientId] = new List<Prescription>();
                _prescriptionMap[rx.PatientId].Add(rx);
            }
        }

        public List<Prescription> GetPrescriptionsByPatientId(int patientId)
        {
            List<Prescription> list;
            if (_prescriptionMap.TryGetValue(patientId, out list))
                return new List<Prescription>(list);
            return new List<Prescription>();
        }

        public void PrintAllPatients()
        {
            Console.WriteLine("\n--- All Patients ---");
            foreach (var p in _patientRepo.GetAll())
            {
                Console.WriteLine(p);
            }
        }

        public void PrintPrescriptionsForPatient(int id)
        {
            Console.WriteLine($"\n--- Prescriptions for PatientId={id} ---");
            var list = GetPrescriptionsByPatientId(id);
            if (list.Count == 0)
            {
                Console.WriteLine("No prescriptions found.");
                return;
            }
            foreach (var rx in list.OrderBy(r => r.DateIssued))
            {
                Console.WriteLine(rx);
            }
        }
    }
        internal class Program
    {
        static void Main(string[] args)
        {
            var app = new HealthSystemApp();
            app.SeedData();
            app.BuildPrescriptionMap();
            app.PrintAllPatients();

            // Select a PatientId and display their prescriptions
            app.PrintPrescriptionsForPatient(2);

            Console.WriteLine("\nDone.");
        }
    }
}
