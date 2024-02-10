using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.IO.Ports;

namespace VotingSystem
{
    internal class VotingMachine
    {
        //------------data members of the class with setter/getter-----------
        private List<Candidate> candidates;

        public List<Candidate> Candidate
        {
            get { return candidates; }
            set { candidates = value; }
        }

        //-------------following are the methods of this class---------

        public VotingMachine()
        {
            candidates = new List<Candidate>();
        }

        //helper function
        private void UpdateVoterInFile(Voter v)
        {
            Voter tempVoter = new Voter();
            using (FileStream fin = new FileStream("Voters.txt", FileMode.Open, FileAccess.Read))
            using (FileStream fout = new FileStream("temp.txt", FileMode.Create, FileAccess.Write))
            using (StreamReader sr = new StreamReader(fin))
            using (StreamWriter sw = new StreamWriter(fout))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    tempVoter = JsonSerializer.Deserialize<Voter>(line);
                    if (tempVoter.Cnic == v.Cnic)
                    {
                        //Update the party name for the specific voter
                        tempVoter.SelectedPartyName = v.SelectedPartyName;
                        tempVoter.VoterName = v.VoterName;
                    }

                    string updatedLine = JsonSerializer.Serialize(tempVoter);
                    sw.WriteLine(updatedLine);
                }
            }
            File.Delete("Voters.txt");
            File.Move("temp.txt", "Voters.txt");
        }

        //helper function
        private void UpdateCandidateInFile(Candidate c)
        {
            Candidate cand = new Candidate();
            using (FileStream fin = new FileStream("candidate.txt", FileMode.Open, FileAccess.Read))
            using (FileStream fout = new FileStream("temp.txt", FileMode.Create, FileAccess.Write))
            using (StreamReader sr = new StreamReader(fin))
            using (StreamWriter sw = new StreamWriter(fout))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    cand = JsonSerializer.Deserialize<Candidate>(line);
                    if (cand.CandidateID == c.CandidateID)
                    {
                        //Update the party name for the specific voter

                        cand.Name = c.Name;
                        cand.Party = c.Party;
                        cand.Votes = c.Votes;
                    }

                    string updatedLine = JsonSerializer.Serialize(cand);
                    sw.WriteLine(updatedLine);
                }
            }
            File.Delete("candidate.txt");
            File.Move("temp.txt", "candidate.txt");
        }

        public void CastVote(Candidate c, Voter v)
        {
            if (!v.hasVoted(v.Cnic))
            {
                Console.WriteLine("-------------\n");
                v.SelectedPartyName = c.Party;
                c.IncrementVotes();
                string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    //updating voters data  
                    string text = "update voter set party = @partyname where VoterId = @ID";
                    using (SqlCommand cmd = new SqlCommand(text, connection))
                    {
                        cmd.Parameters.AddWithValue("@partyname", v.SelectedPartyName);
                        cmd.Parameters.AddWithValue("@ID", v.Cnic);
                        cmd.ExecuteNonQuery();
                    }
                    //updating candidate data in database
                    string query = "update candidate set votes = @new_votes where id = @ID";
                    using (SqlCommand cmd1 = new SqlCommand(query, connection))
                    {
                        cmd1.Parameters.AddWithValue("@new_votes", c.Votes);
                        cmd1.Parameters.AddWithValue("@ID", c.CandidateID);
                        cmd1.ExecuteNonQuery();
                    }
                }
                // Updating data in the file system
                UpdateVoterInFile(v);
                UpdateCandidateInFile(c);
                Console.WriteLine("Your vote is casted successfully");
            }
            else
            {
                Console.WriteLine("You have already casted the vote");
            }
        }

        public bool UpdateVoter(string cnic)
        {
            Console.Write("Enter Updated Name: ");
            string updatedName = Console.ReadLine();
            Console.Write('\n');
            string p_n = "";
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("UPDATE voter SET VoterName = @name WHERE voterId = @id", connection))
                    {
                        cmd.Parameters.AddWithValue("@id", cnic);
                        cmd.Parameters.AddWithValue("@name", updatedName);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Voter updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("No matching voter found for the given CNIC.");
                            return false;
                        }
                    }
                    using (SqlCommand cmd = new SqlCommand("SELECT Party FROM voter WHERE VoterId = @id", connection))
                    {
                        cmd.Parameters.AddWithValue("@id", cnic);

                        // Use ExecuteScalar() to retrieve a single value
                        object result = cmd.ExecuteScalar();

                        // Check if the result is not null before converting to string
                        if (result != null)
                        {
                            p_n = result.ToString();                            
                        }
                        else
                        {
                            p_n = "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating voter: {ex.Message}");
                return false;
            }

            Voter temp = new Voter(updatedName, cnic, p_n);
            UpdateVoterInFile(temp);
            return true;
        }

        public void AddVoter()
        {
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("1: Add Voter");
            Console.WriteLine("---------------------------------------");

            Console.Write("Enter your Name: ");
            string n = Console.ReadLine();
            Console.Write('\n');

            Console.Write("Enter your CNIC: ");
            string c = Console.ReadLine();
            Console.Write('\n');

            //--------------writing data into the database system
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string commandText = "INSERT INTO voter (VoterId, VoterName) VALUES (@id, @name)";
                    using (SqlCommand cmd = new SqlCommand(commandText, connection))
                    {
                        // Add parameters to the query
                        cmd.Parameters.AddWithValue("@id", c);
                        cmd.Parameters.AddWithValue("@name", n);
                        //cmd.Parameters.AddWithValue("@party", p);
                        cmd.ExecuteNonQuery();
                    }
                }

                //------------------writing data into the file stream
                Voter v = new Voter(n, c, "");
                string jsonString = JsonSerializer.Serialize(v);
                using (StreamWriter sw = new StreamWriter("Voters.txt", append: true))
                {
                    sw.WriteLine(jsonString);
                }

                Console.WriteLine("Voter added successfully:");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627)
                {
                    Console.WriteLine("Error: Voter with the same CNIC already exists in the database.");
                }
                else
                {
                    Console.WriteLine($"Error adding voter: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding voter: {ex.Message}");
            }
        }

        public void DisplayVoters()
        {
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("Display Voters");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine("List of Voters");
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select * from voter";
                SqlCommand cmd = new SqlCommand(query, connection);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.Write("Cnic: ");
                    Console.WriteLine(reader.GetString(0));
                    Console.Write("Name: ");
                    Console.WriteLine(reader.GetString(1));
                    if (!reader.IsDBNull(2))
                    {
                        Console.Write("Selected Party: ");
                        Console.WriteLine(reader.GetString(2));
                    }
                    else
                    {
                        Console.Write("Selected Party: ");
                        Console.WriteLine("Party didn't selected yet");
                    }

                }
            }
        }
        public void InsertCandidate(Candidate c)
        {
            try
            {
                candidates.Add(c);

                string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO candidate (id, name, party, votes) VALUES (@id, @name, @party, @votes)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", c.CandidateID);
                        command.Parameters.AddWithValue("@name", c.Name);
                        command.Parameters.AddWithValue("@party", c.Party);
                        command.Parameters.AddWithValue("@votes", c.Votes);
                        command.ExecuteNonQuery();
                    }
                }

                using (StreamWriter sw = new StreamWriter("candidate.txt", append: true))
                {
                    string jsonString = JsonSerializer.Serialize(c);
                    sw.WriteLine(jsonString);
                }

                Console.WriteLine("Candidate Added Successfully");
            }
            catch (SqlException ex)
            {
                // Check if the exception is related to a unique constraint violation
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    Console.WriteLine("Error: Party name must be unique. This party name already exists in the database.");
                }
                else
                {
                    Console.WriteLine($"Error adding candidate: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding candidate: {ex.Message}");
            }
        }

        public void ReadCandidate(int id)
        {
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine("Read Candidate");
            Console.WriteLine("-------------------------------------------");


            Console.WriteLine("Reading data from the database: \n\n");
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "select * from candidate where id = @id";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Check if there are rows returned
                        if (reader.HasRows)
                        {
                            // Read the first row
                            reader.Read();

                            // Access the values using the reader's methods (e.g., GetString, GetInt32, etc.)
                            string candidateName = reader.GetString(reader.GetOrdinal("name"));
                            string party = reader.GetString(reader.GetOrdinal("party"));
                            int votes = reader.GetInt32(reader.GetOrdinal("votes"));

                            // Use the retrieved data as needed
                            Console.WriteLine($"Candidate ID: {id}");
                            Console.WriteLine($"Name: {candidateName}");
                            Console.WriteLine($"Party: {party}");
                            Console.WriteLine($"Votes: {votes}");
                        }
                        else
                        {
                            Console.WriteLine($"Candidate with ID {id} not found.");
                        }
                    }
                }

                Console.WriteLine("\nReading data from the File: \n");
                using (StreamReader sr = new StreamReader("candidate.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        string jsonStr = sr.ReadLine();
                        Candidate cff = JsonSerializer.Deserialize<Candidate>(jsonStr);
                        // Check if the candidate ID matches the desired ID
                        if (cff.CandidateID == id)
                        {
                            Console.WriteLine($"Candidate ID: {id}");
                            Console.WriteLine($"Name: {cff.Name}");
                            Console.WriteLine($"Party: {cff.Party}");
                            Console.WriteLine($"Votes: {cff.Votes}");
                            return; // Found the candidate, exit the loop
                        }
                    }

                    Console.WriteLine($"Candidate with ID {id} not found in the file.");
                }
            }
        }

        public void DisplayCandidates()
        {
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("Display Candidates");
            Console.WriteLine("-------------------------------------------------");

            Console.WriteLine("ID\tNAME\t\tPARTY\t\tVOTES");

            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "select * from candidate";
                SqlCommand cmd = new SqlCommand(query, con);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int candidateID = reader.GetInt32(reader.GetOrdinal("id"));
                        string candidateName = reader.GetString(reader.GetOrdinal("name"));
                        string party = reader.GetString(reader.GetOrdinal("party"));
                        int votes = reader.GetInt32(reader.GetOrdinal("votes"));

                        // Format and display the candidate information
                        Console.WriteLine($"{candidateID}\t{candidateName}\t\t{party}\t\t{votes}");
                    }
                }
            }
        }

        public void DeclareWinner()
        {
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine("Declare Winner");
            Console.WriteLine("-------------------------------------------------");

            Console.Write("Winner is: ");
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            int maxVotes = -100;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "select * from candidate";
                SqlCommand cmd = new SqlCommand(query, con);
                string candidateName = "";
                string party = "";
                int candidateID = 0;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int votes = reader.GetInt32(reader.GetOrdinal("votes"));
                        if (votes > maxVotes)
                        {
                            maxVotes = votes;
                            candidateID = reader.GetInt32(reader.GetOrdinal("id"));
                            candidateName = reader.GetString(reader.GetOrdinal("name"));
                            party = reader.GetString(reader.GetOrdinal("party"));
                        }
                    }
                    Console.WriteLine($"{candidateName}\t{party}\t{maxVotes}");
                }
            }
        }

        public void UpdateCandidate(Candidate c, int id)
        {

            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand("UPDATE candidate SET Name = @name, Party = @p_name WHERE ID = @id", connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@name", c.Name);
                        cmd.Parameters.AddWithValue("@p_name", c.Party);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("Candidate updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("No matching Candidate found for the given ID.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating voter: {ex.Message}");
            }
            UpdateCandidateInFile(c);
        }

        public void DeleteCandidate(int id)
        {
            // Delete from database
            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=VotingSystem;Integrated Security=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM Candidate WHERE ID = @id";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Candidate deleted successfully");
                    }
                    else
                    {
                        Console.WriteLine("No matching Candidate found for the given ID in the database.");
                    }
                }
            }

            // Delete from file
            string tempFileName = "temp.txt";
            Candidate cand = new Candidate();
            using (FileStream fin = new FileStream("candidate.txt", FileMode.Open, FileAccess.Read))
            using (FileStream fout = new FileStream(tempFileName, FileMode.Create, FileAccess.Write))
            using (StreamReader sr = new StreamReader(fin))
            using (StreamWriter sw = new StreamWriter(fout))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    cand = JsonSerializer.Deserialize<Candidate>(line);
                    if (cand.CandidateID == id)
                    {
                        // Skip the line for the candidate to be deleted
                        continue;
                    }

                    string updatedLine = JsonSerializer.Serialize(cand);
                    sw.WriteLine(updatedLine);
                }
            }
            File.Delete("candidate.txt");
            File.Move(tempFileName, "candidate.txt");
        }

    }
}
