using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;

namespace VotingSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            VotingMachine vm = new VotingMachine();
            int choice = 0;
            //String cnic = "";
            while (choice != 11)
            {
                Console.WriteLine("\n1. Add Voter");
                Console.WriteLine("2. Update Voter");
                Console.WriteLine("3. Display Voters");
                Console.WriteLine("4. Cast Vote");
                Console.WriteLine("5. Insert Candidate");
                Console.WriteLine("6. Update Candidate");
                Console.WriteLine("7. Display Candidates");
                Console.WriteLine("8. Delete Candidate");
                Console.WriteLine("9. Declare Winner");
                Console.WriteLine("10. Read Candidate");
                Console.WriteLine("11. Exit Console App");
                Console.WriteLine("Enter your choice: ");
                choice = Convert.ToInt32(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        vm.AddVoter();
                        break;
                    case 2:
                        Console.WriteLine("Enter the Cnic you want to Update: ");
                        string cnic = Console.ReadLine();
                        Console.WriteLine();
                        vm.UpdateVoter(cnic);
                        break;
                    case 3:
                        vm.DisplayVoters();
                        break;
                    case 4:
                        vm.DisplayCandidates();
                        Console.WriteLine("-------Select from the above candidates----------");

                        Console.WriteLine("Enter the party name of the candidate: ");
                        string party = Console.ReadLine();

                        Candidate candidateFromDB = new Candidate();
                        Voter vt = new Voter();
                        string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";

                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            string query = "SELECT id, name, party, votes FROM candidate WHERE party = @party";

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@party", party);

                                using (SqlDataReader reader = command.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        candidateFromDB.CandidateID = reader.GetInt32(0);
                                        candidateFromDB.Name = reader.GetString(1);
                                        candidateFromDB.Party = reader.GetString(2);
                                        candidateFromDB.Votes = reader.GetInt32(3);

                                        Console.WriteLine("Candidate details retrieved from the database:");
                                        Console.WriteLine($"ID: {candidateFromDB.CandidateID}");
                                        Console.WriteLine($"Name: {candidateFromDB.Name}");
                                        Console.WriteLine($"Party: {candidateFromDB.Party}");
                                        Console.WriteLine($"Votes: {candidateFromDB.Votes}");
                                    }
                                    else
                                    {
                                        Console.WriteLine("No candidate found with the given party name.");
                                        // You might want to return or handle this situation appropriately
                                    }
                                }
                            }

                            Console.WriteLine("Enter your CNIC: ");
                            cnic = Console.ReadLine();

                            string text = "SELECT TOP 1 * FROM voter WHERE VoterId = @cnic";
                            using (SqlCommand cmd = new SqlCommand(text, connection))
                            {
                                cmd.Parameters.AddWithValue("@cnic", cnic);

                                using (SqlDataReader dr = cmd.ExecuteReader())
                                {
                                    if (dr.Read())
                                    {
                                        vt.Cnic = dr.GetString(0);
                                        vt.VoterName = dr.GetString(1);
                                        vt.SelectedPartyName = party;

                                        // Now you have the candidateFromDB and voter information, proceed to cast the vote
                                    }
                                    else
                                    {
                                        Console.WriteLine("No entry against the given CNIC");
                                    }
                                }
                            }
                        }
                        vm.CastVote(candidateFromDB, vt);
                        break;  // Exiting the switch statement

                    case 5:
                        Console.WriteLine("Enter the name of the candidate: ");
                        string name = Console.ReadLine();
                        Console.WriteLine("Enter the party name of the candidate: ");
                        party = Console.ReadLine();
                        Candidate cand = new Candidate(name, party);
                        vm.InsertCandidate(cand);
                        break;
                    case 6:
                        Console.WriteLine("---------------------------------------");
                        Console.WriteLine("1: Update Candidate");
                        Console.WriteLine("---------------------------------------");
                        Console.WriteLine("Enter the ID of the candidate to Update: ");
                        int IdUpdate = Convert.ToInt32(Console.ReadLine());
                        Console.WriteLine("Enter new name of the candidate: ");
                        string newName = Console.ReadLine();
                        Console.WriteLine("Enter new party name of the candidate: ");
                        string newParty = Console.ReadLine();
                        Candidate candidateUpdate = new Candidate { CandidateID = IdUpdate, Name = newName, Party = newParty };
                        vm.UpdateCandidate(candidateUpdate, IdUpdate);
                        break;
                    case 7:
                        vm.DisplayCandidates();
                        break;
                    case 8:
                        Console.WriteLine("Enter the ID of the candidate to delete: ");
                        int IdDelete = Convert.ToInt32(Console.ReadLine());
                        vm.DeleteCandidate(IdDelete);
                        break;
                    case 9:
                        vm.DeclareWinner();
                        break;
                    case 10:
                        Console.WriteLine("Enter ID of the candidate:");
                        int id = Convert.ToInt32(Console.ReadLine());
                        vm.ReadCandidate(id);
                        break;
                    case 11:
                        Console.WriteLine("-----------------Good Bye---------------");
                        break;
                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }
            }
        }
    }
}
