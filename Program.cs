using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        //Constant Declarations
        const string CREATE_RECORD = "CREATE_RECORD";
        const string UPDATE_RECORD = "UPDATE_RECORD";
        const string DELETE_RECORD = "DELETE_RECORD";

        const string SUCCESS = "SUCCESS";
        const string FAIL = "FAIL";

        const int BUFFER_SIZE = 100;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello from the server...");

            const int PORT = 50010;

            EndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT);

            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(endPoint);
            server.Listen(5);

            while (true)
            {
                Console.WriteLine("Waiting for a connection...");

                //Accept is a blocking method. A blocking method suspends
                //program execution. Once a client connects, the Accept method
                //returns a reference to a Socket object. We use this new Socket
                //object to communication with the client.
                Socket connection = server.Accept();
                Console.WriteLine("Connected");

                //Saving the remote client's IP address
                IPEndPoint endpoint = connection.RemoteEndPoint as IPEndPoint;
                IPAddress iPAddress = endpoint.Address;

                //Wait for the client to send data. The Receive method is
                //also a blocking method. The program execution is again
                //suspended until the client sends data. When data is sent
                //from the client, the server receives the data, and the
                //Receive method unblocks.
                byte[] buffer = new byte[BUFFER_SIZE];
                int bytesRead = connection.Receive(buffer);

                //Send status code
                SendMessageToClient(connection);

                string messageType = WriteTypeOfFileTransaction(iPAddress, buffer, bytesRead);

                Console.WriteLine("MESSAGE TYPE: " + messageType);

                Console.WriteLine("Bytes read: " + bytesRead);

                //Wait for the client to send product data. 
                buffer = new byte[BUFFER_SIZE];
                bytesRead = connection.Receive(buffer);

                //Determine what the server needs to do based on the client input:
                //1: CREATE RECORD 
                //  A: Just create record

                if (messageType == CREATE_RECORD)
                {
                    CreateProductRecord(buffer, bytesRead, connection);
                }
                else if (messageType == UPDATE_RECORD)
                {
                    UpdateProductRecord(buffer, bytesRead, connection);
                }
                else if (messageType == DELETE_RECORD)
                {
                    DeleteProductRecord(buffer, bytesRead, connection);
                }

                //2: UPDATE RECORD 
                //  A: Open the file
                //  B: Read the entire file into a buffer
                //  C: Find the record that needs to be updated and update the record
                //  D: Delete the existing file, and write back the contents of the buffer
                StreamReader reader = new StreamReader("Product_Records.txt");

                string record = reader.ReadLine();

                while (record != null)
                {
                    //Determine if this is the record that needs updating

                    record = reader.ReadLine();
                }

                //3: DELETE RECORD
                //  A: Open the file
                //  B: Read the entire file into a buffer
                //  C: Find the record that needs to be deleted and delete the record
                //  D: Delete the existing file, and write back the contents of the buffer

                reader = new StreamReader("Product_Records.txt");

                record = reader.ReadLine();

                while (record != null)
                {
                    //Determine if this is the record that needs deleting

                    record = reader.ReadLine();
                }

                //Send status code
                SendMessageToClient(connection);

                connection.Close();

            }

            server.Close();

            Console.WriteLine("Server shut down");

        }

        private static void CreateProductRecord(byte[] buffer, int bytesRead, Socket connection)
        {
            bool overwriteNecessary = false;
            string message = string.Empty;
            string userResponse = string.Empty;
            string record = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            string newRecord = string.Empty;

            //Reading the file
            StreamReader reader = new StreamReader("Product_Records.txt");
            string fileContent = reader.ReadToEnd();
            reader.Close();

            //Parse the records
            char[] lineSeperators = new char[] { ';' };
            string[] records = fileContent.Split(lineSeperators);

            //For each record, parse the record files
            for (int recordCount = 0; recordCount < records.Length; recordCount++)
            {
                //splitting it from newlines
                lineSeperators = new char[] { '\n' };
                string[] splitLineRecordFields = records[recordCount].Split(lineSeperators);
                char[] commaSeperators = new char[] { ',' };

                //splitting it from commas
                for (int i = 0; i < splitLineRecordFields.Length; i++)
                {
                    string[] splitCommaRecordFields = splitLineRecordFields[i].Split(commaSeperators);
                    if (record[0].ToString() == splitCommaRecordFields[0].ToString())
                    {
                        overwriteNecessary = true;
                        break;
                    }
                }                            
            }
            Console.WriteLine(message);

            if (overwriteNecessary)
            {
                message = "1";                
            }
            else
            {
                message = "0";
            }

            SendMessageToClient(connection, message);

            if (overwriteNecessary)
            {
                byte[] clientResponseYN = new byte[BUFFER_SIZE];
                int clientResponseYNRead = connection.Receive(clientResponseYN);

                userResponse = Encoding.ASCII.GetString(clientResponseYN, 0, clientResponseYNRead);

                if (userResponse == "Y")
                {
                    UpdateProductRecord(buffer, bytesRead, connection);
                    return;
                }
                else
                {
                    return;
                }
            }

            StreamWriter writer = new StreamWriter("Product_Records.txt", true);
            writer.Write("\n");
            writer.Write(record);
            writer.Flush();

            writer.Close();
        }
        private static void UpdateProductRecord(byte[] buffer, int bytesRead, Socket connection)
        {
            bool overwriteNecessary = false;
            string message = string.Empty;
            string userResponse = string.Empty;
            string record = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            string newRecord = string.Empty;

            //create the file if needed
            StreamWriter tempWriter = new StreamWriter("Product_Records.txt");
            tempWriter.Close();

            //Reading the file
            StreamReader reader = new StreamReader("Product_Records.txt");
            string fileContent = reader.ReadToEnd();
            reader.Close();

            //Parse the records
            char[] lineSeperators = new char[] { ';' };
            string[] records = fileContent.Split(lineSeperators);

            //For each record, parse the record files
            for (int recordCount = 0; recordCount < records.Length; recordCount++)
            {
                //splitting it from newlines
                lineSeperators = new char[] { '\n' };
                string[] splitLineRecordFields = records[recordCount].Split(lineSeperators);
                char[] commaSeperators = new char[] { ',' };

                //splitting it from commas
                for (int i = 0; i < splitLineRecordFields.Length; i++)
                {
                    string[] splitCommaRecordFields = splitLineRecordFields[i].Split(commaSeperators);

                    if (record[0].ToString() == splitCommaRecordFields[0].ToString())
                    {
                        StreamWriter bufferFile = new StreamWriter("Buffer.txt", true);
                        bufferFile.Flush();

                        for (int j = 0; j < splitLineRecordFields.Length; j++)
                        {
                            if (splitLineRecordFields[i] != splitLineRecordFields[j] && splitLineRecordFields[i] != string.Empty)
                            {
                                string bufferString = string.Empty;
                                bufferString = splitLineRecordFields[j];
                                bufferString += "\n";

                                bufferFile.Write(bufferString);
                                bufferFile.Flush();
                            }
                        }

                        bufferFile.Write(record);
                        bufferFile.Flush();

                        bufferFile.Close();


                        File.Replace("Buffer.txt", "Product_Records.txt", "DeleteThisPlease.txt");

                        File.Delete("Buffer.txt");
                        File.Delete("DeleteThisPlease.txt");




                        return;               
                    }                    
                }
            }
            if (overwriteNecessary)
            {
                message = "1";
            }
            else
            {
                message = "0";
            }

            SendMessageToClient(connection, message);

            if (!overwriteNecessary)
            {
                byte[] clientResponseYN = new byte[BUFFER_SIZE];
                int clientResponseYNRead = connection.Receive(clientResponseYN);

                userResponse = Encoding.ASCII.GetString(clientResponseYN, 0, clientResponseYNRead);

                if (userResponse == "Y")
                {
                    CreateProductRecord(buffer, bytesRead, connection);
                    return;
                }
                else
                {
                    return;
                }
            }
        }
        private static void DeleteProductRecord(byte[] buffer, int bytesRead, Socket connection)
        {
            string message = string.Empty;
            string userResponse = string.Empty;
            string record = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            string newRecord = string.Empty;

            //Reading the file
            StreamReader reader = new StreamReader("Product_Records.txt");
            string fileContent = reader.ReadToEnd();
            reader.Close();

            //Parse the records
            char[] lineSeperators = new char[] { ';' };
            string[] records = fileContent.Split(lineSeperators);

            //For each record, parse the record files
            for (int recordCount = 0; recordCount < records.Length; recordCount++)
            {
                //splitting it from newlines
                lineSeperators = new char[] { '\n' };
                string[] splitLineRecordFields = records[recordCount].Split(lineSeperators);
                char[] commaSeperators = new char[] { ',' };

                //splitting it from commas
                for (int i = 0; i < splitLineRecordFields.Length; i++)
                {
                    string[] splitCommaRecordFields = splitLineRecordFields[i].Split(commaSeperators);

                    if (record[0].ToString() == splitCommaRecordFields[0].ToString())
                    {
                        StreamWriter bufferFile = new StreamWriter("Buffer.txt", true);
                        bufferFile.Flush();

                        for (int j = 0; j < splitLineRecordFields.Length; j++)
                        {
                            if (splitLineRecordFields[i] != splitLineRecordFields[j] && splitLineRecordFields[i] != string.Empty)
                            {
                                string bufferString = string.Empty;
                                bufferString = splitLineRecordFields[j];
                                bufferString += "\n";

                                bufferFile.Write(bufferString);
                                bufferFile.Flush();
                            }
                        }

                        bufferFile.Close();
                        File.Replace("Buffer.txt", "Product_Records.txt", "DeleteThisPlease.txt");

                        File.Delete("Buffer.txt");
                        File.Delete("DeleteThisPlease.txt");


                        return;
                    }
                }
            }
        }

        private static string WriteTypeOfFileTransaction(IPAddress iPAddress, byte[] buffer, int bytesRead)
        {
            //Save the information about the type of file operation
            string messageType = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            DateTime dateTime = DateTime.Now;
            string currentDataTime = dateTime.ToString();

            StreamWriter writer = new StreamWriter("Transactions_Records.txt", true);

            string record = iPAddress + "," + currentDataTime + "," + messageType + Environment.NewLine;
            writer.Write(record);
            writer.Flush();

            writer.Close();
            return messageType;
        }

        private static void SendMessageToClient(Socket connection)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(SUCCESS);

            int sentBytes = connection.Send(buffer);
        }
        private static void SendMessageToClient(Socket connection, string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);

            int sentBytes = connection.Send(buffer);
        }
    }
}
