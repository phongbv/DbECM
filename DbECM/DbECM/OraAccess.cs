using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OracleClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbECM
{
    public class FileInfo
    {
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
    }
    public class OraAccess
    {
        private OracleConnection _oracleConn;
        private OracleCommand _oracleCommand;
        private string insCommand = "insert into FILE_ECM(LOS_ID,FILE_CONTENT,FILE_NAME,EXTRA_INFO,CONTENT_TYPE)" +
                                    " values(:ilos_id,:ifile_content,:ifile_name,:iextra_info,:content_type) returning  to_number(ECM_ID) into :oecm_id";
        private string selEcmIdCommand = "select LOS_ID,FILE_CONTENT,FILE_NAME,CONTENT_TYPE from FILE_ECM where ECM_ID = :iecm_id";
        private string updateEcmById = "update FILE_ECM set FILE_CONTENT = :ifile_content where ECM_ID = :iecm_id";
        private string uniqueId;
        public string ConnStr
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["DbECMConnection"].ConnectionString;

            }
        }
        public FileInfo Download(string ecmId)
        {
            FileInfo fileResult = new FileInfo();
            using (_oracleConn = new OracleConnection(ConnStr))
            {
                try
                {
                    _oracleConn.Open();
                    OracleParameter param = new OracleParameter();
                    _oracleCommand = new OracleCommand(selEcmIdCommand);
                    _oracleCommand.Connection = _oracleConn;
                    param = _oracleCommand.Parameters.Add("iecm_id", OracleType.VarChar);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = ecmId;

                    // Execute command select to OracleDataReader to database
                    OracleDataReader _oraReader = null;
                    _oraReader = _oracleCommand.ExecuteReader();
                    if (_oraReader.HasRows)
                    {
                        _oraReader.Read();
                        var bfile = _oraReader.GetOracleLob(_oraReader.GetOrdinal("FILE_CONTENT"));
                        fileResult.FileName = _oraReader.GetString(_oraReader.GetOrdinal("FILE_NAME"));
                        fileResult.FileContent = Encoding.Default.GetBytes((string)bfile.Value);
                        return fileResult;
                    }
                    return null;

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    _oracleConn.Close();
                }
            }
        }

        public void Update(string ecmId, byte[] fileContent)
        {
            using (_oracleConn = new OracleConnection(ConnStr))
            {
                try
                {
                    _oracleConn.Open();
                    OracleParameter param = new OracleParameter();
                    _oracleCommand = new OracleCommand(updateEcmById);
                    _oracleCommand.Connection = _oracleConn;
                    param = _oracleCommand.Parameters.Add("iecm_id", OracleType.VarChar);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = ecmId;
                    param = _oracleCommand.Parameters.Add("ifile_content", OracleType.Clob);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = Encoding.Default.GetString(fileContent);
                    _oracleCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    _oracleConn.Close();
                }
            }
        }

    }
}
