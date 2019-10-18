using System;
using System.Globalization;
using System.IO;

namespace WindowsFormsApp1 { 
    public class JobLogger { 
        private readonly bool _logToFile;
        private readonly bool _logToConsole;
        private readonly bool _logToDatabase;
        private readonly bool _logInfo;
        private readonly bool _logWarning;
        private readonly bool _logError;
        private readonly bool _initialized;
        private readonly System.Data.SqlClient.SqlConnection connection;
        
        //Constructors and destructor
        public JobLogger() {
            this._logToFile = false;
            this._logToConsole = false;
            this._logToDatabase = false;
            this._logInfo = false;
            this._logWarning = false;
            this._logError = false;
            this._initialized = false;
        }
        public JobLogger(bool logToFile, bool logToConsole, bool logToDatabase, bool logInfo, bool logWarning, bool logError) {
            this._logToFile = logToFile;
            this._logToConsole = logToConsole;
            this._logToDatabase = logToDatabase;
            this._logInfo = logInfo;
            this._logWarning = logWarning;
            this._logError = logError;
            this._initialized = true;
            if (this._logToDatabase) {
                this.connection = new System.Data.SqlClient.SqlConnection(System.Configuration.ConfigurationManager.AppSettings["ConnectionString"]);
                connection.Open();
            }
        }
        ~JobLogger() {
            if (this._logToDatabase)
                this.connection.Close();
        }

        //Getters for bool flags
        public bool is_logToFile() {
            return this._logToFile;
        }
        public bool is_logToConsole() {
            return this._logToConsole;
        }
        public bool is_logToDatabase() {
            return this._logToDatabase;
        }
        public bool is_logInfo() {
            return this._logInfo;
        }
        public bool is_logWarning() {
            return this._logWarning;
        }
        public bool is_logError() {
            return this._logError;
        }
        public bool is_initialized() {
            return this._initialized;
        }

        //Types of messages and fuction for creating an output string
        public const int INFO = 0;
        public const int WARNING = 1;
        public const int ERROR = 2;

        private static string Make_String(string msg, int msg_type) {
            string ret = String.Format("[{0}] ",
                DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
            if (msg_type == JobLogger.INFO)
                ret += "INFO: ";
            else if (msg_type == JobLogger.WARNING)
                ret += "WARNING: ";
            else if (msg_type == JobLogger.ERROR)
                ret += "ERROR: ";
            ret += msg;
            return ret;
        }

        public const int CONSOLE_SHIFT = 0;
        public const int FILE_SHIFT = 1;
        public const int DB_SHIFT = 2;
        // LogMessage method for our JobLogger instance, completely rewritten
        // Returns bitMask, which shows output streams affected
        // zero bit -> shows, if console output appeared
        // 1-st bit -> shows, if file ouput appeared
        // 2-st bit -> shows, if DataBase output appeared
        public int LogMessage(string msg, int msg_type) {
            if (msg_type != JobLogger.INFO && msg_type != JobLogger.WARNING
                && msg_type != JobLogger.ERROR)
                throw new Exception("ERROR or WARNING or MESSAGE msg_type must be specified");
            //If msg_type should not be processed, according to _logInfo, _logWarning and _logError flags,
            //then return 0 -> no output should be done at all
            if (msg_type == INFO && !this._logInfo || msg_type == WARNING && !this._logWarning
                || msg_type == ERROR && !this._logError)
                return 0;

            int ret = 0;
            string Output = JobLogger.Make_String(msg, msg_type);
            string Out_Path = System.Configuration.ConfigurationManager
                .AppSettings["Log FileDirectory"] + "LogFile.txt";
            if (this._logToDatabase) {
                string buf = String.Format("INSERT INTO Log (Records) VALUES ('{0}')", Output);
                System.Data.SqlClient.SqlCommand command = new System.Data.SqlClient.SqlCommand(
                    buf, this.connection);
                command.ExecuteNonQuery();
                ret += 1 << JobLogger.DB_SHIFT;
            }
            if (this._logToFile) {
                File.AppendAllText(Out_Path, Output + Environment.NewLine);
                ret += 1 << JobLogger.FILE_SHIFT;
            }
            if(this._logToConsole) {
                if(msg_type == INFO)
                    Console.ForegroundColor = ConsoleColor.White;
                else if(msg_type == WARNING)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if(msg_type == ERROR)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(Output);
                ret += 1 << JobLogger.CONSOLE_SHIFT;
            }
            return ret;
        }
    }
}