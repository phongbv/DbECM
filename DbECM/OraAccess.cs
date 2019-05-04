using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private const string insCommand = "insert into FILE_ECM(LOS_ID,FILE_CONTENT,FILE_NAME,EXTRA_INFO,CONTENT_TYPE)" +
                                    " values(:ilos_id,:ifile_content,:ifile_name,:iextra_info,:content_type) returning  to_number(ECM_ID) into :oecm_id";
        private const string selEcmIdCommand = "select LOS_ID,FILE_CONTENT,FILE_NAME,CONTENT_TYPE from FILE_ECM where ECM_ID = :iecm_id";
        private const string updateEcmById = "update FILE_ECM set FILE_CONTENT = :ifile_content where ECM_ID = :iecm_id";
        private const string updateFileInfoById = "update FILE_INFO set ECM_DOC_ID = :iecm_id where ID = :ID";
        private const string createFileItem = @"INSERT INTO FILE_INFO (ID, UPD_SEQ,
                                       AMND_STATE,
                                       NAME,
                                       TITLE,
                                       TYPE,
                                       SYS_TRAN_ID,
                                       UPLOAD_BY,
                                       EXTENTION,
                                       FILE_SIZE,
                                       SIZE_UNIT,
                                       ECM_DOC_ID)
     VALUES (FILE_INFO_SEQ.nextval, 1,
             'F',
             :FILENAME,
             :FILENAME,
             'Document',
             1000,
             (SELECT id
                FROM sys_user
               WHERE amnd_state = 'F' AND ROWNUM = 1),
             :EXTENSION,
             :FILESIZE,
             'KB',
             1) RETURNING ID INTO :fileId";
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
                    param = _oracleCommand.Parameters.Add("iecm_id", OracleDbType.Varchar2);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = ecmId;

                    // Execute command select to OracleDataReader to database
                    OracleDataReader _oraReader = null;
                    _oraReader = _oracleCommand.ExecuteReader();
                    if (_oraReader.HasRows)
                    {
                        _oraReader.Read();
                        var bfile = _oraReader.GetOracleClob(_oraReader.GetOrdinal("FILE_CONTENT"));
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
                    param = _oracleCommand.Parameters.Add("iecm_id", OracleDbType.Varchar2);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = ecmId;
                    param = _oracleCommand.Parameters.Add("ifile_content", OracleDbType.Clob);
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

        internal void UpdateFileInfo(decimal losId, decimal ecmId)
        {
            using (_oracleConn = new OracleConnection(ConnStr))
            {
                try
                {
                    _oracleConn.Open();
                    OracleParameter param = new OracleParameter();
                    _oracleCommand = new OracleCommand(updateFileInfoById);
                    _oracleCommand.Connection = _oracleConn;
                    param = _oracleCommand.Parameters.Add("iecm_id", OracleDbType.Decimal);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = ecmId;
                    param = _oracleCommand.Parameters.Add("id", OracleDbType.Decimal);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = losId;
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

        internal decimal CreateFileInfo(string fileName, string extesion, long fileSize)
        {
            using (_oracleConn = new OracleConnection(ConnStr))
            {
                try
                {
                    _oracleConn.Open();
                    OracleParameter param = new OracleParameter();
                    _oracleCommand = new OracleCommand(createFileItem);
                    _oracleCommand.BindByName = true;
                    _oracleCommand.Connection = _oracleConn;
                    param = _oracleCommand.Parameters.Add("FILENAME", OracleDbType.NVarchar2);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = fileName;
                    param = _oracleCommand.Parameters.Add("EXTENSION", OracleDbType.Varchar2);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = extesion;
                    param = _oracleCommand.Parameters.Add("FILESIZE", OracleDbType.Decimal);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = fileSize;
                    param = _oracleCommand.Parameters.Add("fileId", OracleDbType.Decimal);
                    param.Direction = System.Data.ParameterDirection.Output;
                    _oracleCommand.ExecuteNonQuery();
                    var tm = decimal.Parse(_oracleCommand.Parameters["fileId"].Value.ToString());
                    return tm;
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

        public decimal Upload(decimal LosID, string FileName, string ExtraInfor, byte[] FileContent, string contentType)
        {
            using (_oracleConn = new OracleConnection(ConnStr))
            {
                try
                {
                    _oracleConn.Open();
                    _oracleCommand = new OracleCommand(insCommand);
                    _oracleCommand.Connection = _oracleConn;
                    // LOS_ID field
                    var param = _oracleCommand.Parameters.Add("ilos_id", OracleDbType.Varchar2);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = LosID;
                    // FILE_CONTENT field
                    param = _oracleCommand.Parameters.Add("ifile_content", OracleDbType.Clob);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = Encoding.Default.GetString(FileContent);
                    // FILE NAME field
                    param = _oracleCommand.Parameters.Add("ifile_name", OracleDbType.Varchar2);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = FileName;
                    // DESCRIPTION field
                    param = _oracleCommand.Parameters.Add("iextra_info", OracleDbType.Varchar2);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = ExtraInfor;
                    // CONTENT_TYPE field
                    param = _oracleCommand.Parameters.Add("content_type", OracleDbType.Varchar2);
                    param.Direction = System.Data.ParameterDirection.Input;
                    param.Value = contentType;
                    // ECM_ID field
                    param = _oracleCommand.Parameters.Add("oecm_id", OracleDbType.Decimal);
                    param.Direction = System.Data.ParameterDirection.Output;
                    param.SourceColumn = "ECM_ID";
                    // Execute command insert to database
                    _oracleCommand.ExecuteNonQuery();
                    return int.Parse(_oracleCommand.Parameters["oecm_id"].Value.ToString());
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
