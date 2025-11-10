using System;
using System.Collections.Generic;

using MySql.Data.MySqlClient;
using System.Data;
//using System.Data.OleDb;
using System.Configuration;

namespace AccountMake
{
    public partial class DBAccess
    {
        #region<フィールド変数>

        #region<mySQL接続情報>
        //static string strServer = "133.8.68.51";
        //static string strUser = "account";
        //static string strPassword = "Qus@zu0n1K";
        //static string strDBName = "account";
        //configファイルから接続情報を取得
        static string strServer = ConfigurationManager.ConnectionStrings["ServerIP"].ToString().Trim();
        static string strUser = ConfigurationManager.ConnectionStrings["User"].ToString().Trim();
        static string strPassword = ConfigurationManager.ConnectionStrings["Pass"].ToString().Trim();
        static string strDBName = ConfigurationManager.ConnectionStrings["DBName"].ToString().Trim();
        static string strAppUsr = ConfigurationManager.ConnectionStrings["ApplicationUser"].ToString().Trim();
        public MySqlConnection myCon = null;
        #endregion

        #region<AccessDB接続情報 mysql化のためコメントアウト>
        //public OleDbConnection AccCon = new OleDbConnection();
        //OleDbCommand AccCom = new OleDbCommand();
        #endregion

        #endregion

        #region<コンストラクタ>
        public DBAccess()
        {
            //mySQL接続用オブジェクト作成
            string strConStr = "Server=" + strServer + ";Database=" + strDBName + ";Uid=" + strUser + ";Pwd=" + strPassword;
            myCon = new MySqlConnection(strConStr);
        }
        #endregion

        #region<テーブル一覧取得>
        public List<string> getTableList()
        {
            string strSql = "show tables";
            List<string> lCmbDt = new List<string>();

            myCon.Open();

            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(strSql, myCon);
            da.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lCmbDt.Add(dt.Rows[i][0].ToString());
            }

            myCon.Close();

            return lCmbDt;
        }
        #endregion

        #region<id_number取得>
        public List<dtclsIdNumber> getIdNumber(string strTableName)
        {
            myCon.Open();

            string strSql = "select * from " + strTableName;
            List<dtclsIdNumber> lIdNumber = new List<dtclsIdNumber>();

            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(strSql, myCon);
            da.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lIdNumber.Add(new dtclsIdNumber {

                    職員番号 = (dt.Rows[i][0] == null ? "" : dt.Rows[i][0].ToString().Trim()),
                    氏名 = (dt.Rows[i][1] == null ? "" : dt.Rows[i][1].ToString().Trim()),
                    職種 = (dt.Rows[i][2] == null ? "" : dt.Rows[i][2].ToString().Trim()),
                    所属 = (dt.Rows[i][3] == null ? "" : dt.Rows[i][3].ToString().Trim()),
                    係講座等 = (dt.Rows[i][4] == null ? "" : dt.Rows[i][4].ToString().Trim()),
                    生年月日 = (dt.Rows[i][5] == null ? "" : dt.Rows[i][5].ToString().Trim())

                });
            }

            myCon.Close();

            return lIdNumber;
        }
        #endregion

        #region<request取得>
        public List<dtclsRequest> getRequest(string strusr,string strNo, string strsei, string strmei, string strfaculty, string strNaisen, string strTable, bool bDevflg)
        {
            myCon.Open();

            //string strSql = "select * from request";
            string strSql = "select order_number,user_jasn,user_jaGivenName,user_sn,user_givenName,occupation,faculty,dept,id_number," +
                            "naisen,mail,user_id,password,jimu_flag,re_application,ins_time,tellephone from " + strTable;
            string strWhere = string.Empty;

            //ユーザID
            if(strusr != string.Empty)
            {
                strWhere += " where user_id = '" + strusr + "' ";
            }

            //職員番号
            if (strNo != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where id_number like '%" + strNo + "%' ";
                }
                else
                {
                    strWhere += " and id_number like '%" + strNo + "%' ";
                }
            }

            //漢字姓
            if (strsei != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where user_jasn like '%" + strsei + "%' ";
                }
                else
                {
                    strWhere += " and user_jasn like '%" + strsei + "%' ";
                }
            }

            //漢字名
            if (strmei != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where user_jaGivenName like '%" + strmei + "%' ";
                }
                else
                {
                    strWhere += " and user_jaGivenName like '%" + strmei + "%' ";
                }
            }

            //所属
            if (strfaculty != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where faculty like '%" + strfaculty + "%' ";
                }
                else
                {
                    strWhere += " and faculty like '%" + strfaculty + "%' ";
                }
            }

            //内線番号
            if (strNaisen != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where naisen like '%" + strNaisen + "%' ";
                }
                else
                {
                    strWhere += " and naisen like '%" + strNaisen + "%' ";
                }
            }

            //事務フラグ(使用者が基盤係の場合は事務フラグがtrueのデータしかださない)
            //基盤係が申請登録をDBに直接入れてるので条件ちょっと変える
            //参照モード時は全部表示する
            if (strAppUsr == "jimu")
            {
                if (strWhere == string.Empty)
                {
                    // strWhere += " where jimu_flag = 'TRUE' ";
                    strWhere += " where jimu_flag = 'TRUE' and mail = 'kk-ajyoho2@jimu.gunma-u.ac.jp' ";
                }
                else
                {
                    strWhere += " and jimu_flag = 'TRUE' and mail = 'kk-ajyoho2@jimu.gunma-u.ac.jp' ";
                }
            }
            else if (strAppUsr == "media")
            {
                //事務職員のデータを見たいとき用にチェックボックスのチェック有無で表示を切り替える
                if (bDevflg)
                {
                    if (strWhere == string.Empty)
                    {
                        strWhere += " where (jimu_flag = 'TRUE'  or mail = 'kk-ajyoho2@jimu.gunma-u.ac.jp') ";
                    }
                    else
                    {
                        strWhere += " and (jimu_flag = 'TRUE'  or mail = 'kk-ajyoho2@jimu.gunma-u.ac.jp') ";
                    }
                }
                else
                {
                    if (strWhere == string.Empty)
                    {
                        strWhere += " where (jimu_flag = 'FALSE'  or mail <> 'kk-ajyoho2@jimu.gunma-u.ac.jp') ";
                    }
                    else
                    {
                        strWhere += " and (jimu_flag = 'FALSE'  or mail <> 'kk-ajyoho2@jimu.gunma-u.ac.jp') ";
                    }
                }
            }


            if (strWhere != string.Empty)
            {
                strSql += strWhere;
            }

            List<dtclsRequest> lRequest = new List<dtclsRequest>();

            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(strSql, myCon);
            da.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string strInDate = dt.Rows[i][15] == null ? "" : dt.Rows[i][15].ToString().Trim();
                bool bTarget = false;

                //入力日付が当日のものは対象処理をture
                if(strInDate.Contains(DateTime.Now.ToString().Substring(0,10)))
                {
                    bTarget = true;
                }

                lRequest.Add(new dtclsRequest
                {
                    処理対象 = bTarget,
                    申請番号 = (dt.Rows[i][0] == null ? "" : dt.Rows[i][0].ToString().Trim()),
                    姓_漢字 = (dt.Rows[i][1] == null ? "" : dt.Rows[i][1].ToString().Trim()),
                    名_漢字 = (dt.Rows[i][2] == null ? "" : dt.Rows[i][2].ToString().Trim()),
                    姓_英字 = (dt.Rows[i][3] == null ? "" : dt.Rows[i][3].ToString().Trim()),
                    名_英字 = (dt.Rows[i][4] == null ? "" : dt.Rows[i][4].ToString().Trim()),
                    職種 = (dt.Rows[i][5] == null ? "" : dt.Rows[i][5].ToString().Trim()),
                    所属 = (dt.Rows[i][6] == null ? "" : dt.Rows[i][6].ToString().Trim()),
                    係講座等 = (dt.Rows[i][7] == null ? "" : dt.Rows[i][7].ToString().Trim()),
                    職員番号 = (dt.Rows[i][8] == null ? "" : dt.Rows[i][8].ToString().Trim()),
                    内線番号 = (dt.Rows[i][9] == null ? "" : dt.Rows[i][9].ToString().Trim()),
                    電話番号 = (dt.Rows[i][16] == null ? "" : dt.Rows[i][16].ToString().Trim()),
                    連絡先 = (dt.Rows[i][10] == null ? "" : dt.Rows[i][10].ToString().Trim()),
                    ユーザID = (dt.Rows[i][11] == null ? "" : dt.Rows[i][11].ToString().Trim()),
                    パスワード = (dt.Rows[i][12] == null ? "" : dt.Rows[i][12].ToString().Trim()),
                    事務フラグ = (dt.Rows[i][13] == null ? "" : dt.Rows[i][13].ToString().Trim()),
                    再申請 = (dt.Rows[i][14] == null ? "" : dt.Rows[i][14].ToString().Trim()),
                    入力日時 = strInDate
                });
            }

            myCon.Close();

            return lRequest;
        }
        #endregion

        #region<Access接続文字列作成 mysql化のためコメントアウト>
        //public void setAccConnection(string strConPath)
        //{
        //    AccCon.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strConPath;
        //}
        #endregion

        #region<所属変換テーブル取得>
        public List<dtclsConvert> getConvertMst(string strShozok,string strKakari,string strSyoku,string strID,bool bMstFlg)
        {
            //AccCon.Open();
            myCon.Open();

            //string strSql = "Select * from 変換テーブル";
            string strSql = "Select * from ConversionTable";
            string strWhere = string.Empty;

            if (bMstFlg)
            {
                if (strShozok != string.Empty)
                {
                    //strWhere += " where 人事所属 like '%" + strShozok + "%' ";
                    strWhere += " where jimu_faculty like '%" + strShozok + "%' ";
                }

                if (strKakari != string.Empty)
                {
                    if (strWhere == string.Empty)
                    {
                        //strWhere += " where 人事係講座 like '%" + strKakari + "%' ";
                        strWhere += " where jimu_dept like '%" + strKakari + "%' ";
                    }
                    else
                    {
                        //strWhere += " and 人事係講座 like '%" + strKakari + "%' ";
                        strWhere += " and jimu_dept like '%" + strKakari + "%' ";
                    }
                }

                if (strSyoku != string.Empty)
                {
                    if (strWhere == string.Empty)
                    {
                        //strWhere += " where 人事職種 like '%" + strSyoku + "%' ";
                        strWhere += " where jimu_occupation like '%" + strSyoku + "%' ";
                    }
                    else
                    {
                        //strWhere += " and 人事職種 like '%" + strSyoku + "%' ";
                        strWhere += " and jimu_occupation like '%" + strSyoku + "%' ";
                    }
                }

                if (strID != string.Empty)
                {
                    if (strWhere == string.Empty)
                    {
                        //strWhere += " where ID = " + strID;
                        strWhere += " where id = " + strID;
                    }
                }
            }
            else
            {
                if (strShozok != string.Empty)
                {
                    //strWhere += " where 人事所属 = '" + strShozok + "' ";
                    strWhere += " where jimu_faculty = '" + strShozok + "' ";
                }

                //空の場合があるので空の時はNULLで検索
                if (strKakari != string.Empty)
                {
                    if (strWhere == string.Empty)
                    {
                        //strWhere += " where 人事係講座 = '" + strKakari + "' ";
                        strWhere += " where jimu_dept = '" + strKakari + "' ";
                    }
                    else
                    {
                        //strWhere += " and 人事係講座 = '" + strKakari + "' ";
                        strWhere += " and jimu_dept = '" + strKakari + "' ";
                    }
                }
                else
                {
                    if (strWhere == string.Empty)
                    {
                        //strWhere += " where 人事係講座 IS NULL ";
                        strWhere += " where jimu_dept IS NULL ";
                    }
                    else
                    {
                        //strWhere += " and 人事係講座 IS NULL ";
                        strWhere += " and jimu_dept IS NULL ";
                    }
                }

                if (strSyoku != string.Empty)
                {
                    if (strWhere == string.Empty)
                    {
                        //strWhere += " where 人事職種 = '" + strSyoku + "' ";
                        strWhere += " where jimu_occupation = '" + strSyoku + "' ";
                    }
                    else
                    {
                        //strWhere += " and 人事職種 = '" + strSyoku + "' ";
                        strWhere += " and jimu_occupation = '" + strSyoku + "' ";
                    }
                }

                if (strID != string.Empty)
                {
                    if (strWhere == string.Empty)
                    {
                        //strWhere += " where ID = " + strID;
                        strWhere += " where id = " + strID;
                    }
                }
            }

            if (strWhere != string.Empty)
            {
                strSql += strWhere;
            }

            //strSql += " order by ID";
            strSql += " order by id";

            //AccCom.CommandText = strSql;
            //AccCom.Connection = AccCon;
            //OleDbDataReader reader = AccCom.ExecuteReader();
            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(strSql, myCon);
            da.Fill(dt);

            List<dtclsConvert> lConvert = new List<dtclsConvert>();

            //while(reader.Read())
            //{
            //    lConvert.Add(new dtclsConvert
            //    {
            //        ID = (reader.GetValue(0) == DBNull.Value ? 0 : (int)reader.GetValue(0)),
            //        人事所属 = (reader.GetValue(1) == DBNull.Value ? "" : reader.GetValue(1).ToString().Trim()),
            //        人事係講座 = (reader.GetValue(2) == DBNull.Value ? "" : reader.GetValue(2).ToString().Trim()),
            //        人事職種 = (reader.GetValue(3) == DBNull.Value ? "" : reader.GetValue(3).ToString().Trim()),
            //        全学所属 = (reader.GetValue(4) == DBNull.Value ? "" : reader.GetValue(4).ToString().Trim()),
            //        全学学科 = (reader.GetValue(5) == DBNull.Value ? "" : reader.GetValue(5).ToString().Trim()),
            //        全学職種 = (reader.GetValue(6) == DBNull.Value ? "" : reader.GetValue(6).ToString().Trim()),
            //        AA荒牧D = (reader.GetValue(7) == DBNull.Value ? "" : reader.GetValue(7).ToString().Trim()),
            //        AA荒牧部 = (reader.GetValue(8) == DBNull.Value ? "" : reader.GetValue(8).ToString().Trim()),
            //        AA昭和D = (reader.GetValue(9) == DBNull.Value ? "" : reader.GetValue(9).ToString().Trim()),
            //        AA昭和部 = (reader.GetValue(10) == DBNull.Value ? "" : reader.GetValue(10).ToString().Trim()),
            //        AA桐生D = (reader.GetValue(11) == DBNull.Value ? "" : reader.GetValue(11).ToString().Trim()),
            //        AA桐生部 = (reader.GetValue(12) == DBNull.Value ? "" : reader.GetValue(12).ToString().Trim())
            //    });
            //}

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lConvert.Add(new dtclsConvert
                {
                    ID = (dt.Rows[i][0] == null ? 0 : (int)dt.Rows[i][0]),
                    人事所属 = (dt.Rows[i][1] == null ? "" : dt.Rows[i][1].ToString().Trim()),
                    人事係講座 = (dt.Rows[i][2] == null ? "" : dt.Rows[i][2].ToString().Trim()),
                    人事職種 = (dt.Rows[i][3] == null ? "" : dt.Rows[i][3].ToString().Trim()),
                    全学所属 = (dt.Rows[i][4] == null ? "" : dt.Rows[i][4].ToString().Trim()),
                    全学学科 = (dt.Rows[i][5] == null ? "" : dt.Rows[i][5].ToString().Trim()),
                    全学職種 = (dt.Rows[i][6] == null ? "" : dt.Rows[i][6].ToString().Trim()),
                    AA荒牧D = (dt.Rows[i][7] == null ? "" : dt.Rows[i][7].ToString().Trim()),
                    AA荒牧部 = (dt.Rows[i][8] == null ? "" : dt.Rows[i][8].ToString().Trim()),
                    AA昭和D = (dt.Rows[i][9] == null ? "" : dt.Rows[i][9].ToString().Trim()),
                    AA昭和部 = (dt.Rows[i][10] == null ? "" : dt.Rows[i][10].ToString().Trim()),
                    AA桐生D = (dt.Rows[i][11] == null ? "" : dt.Rows[i][11].ToString().Trim()),
                    AA桐生部 = (dt.Rows[i][12] == null ? "" : dt.Rows[i][12].ToString().Trim())
                });
            }

            //AccCon.Close();
            myCon.Close();

            return lConvert;

        }
        #endregion

        #region<変換テーブル存在チェック>
        public bool chkConvertMst(string strShozok, string strKakari, string strSyoku, string strZShozok, string strZKakari, string strZSyoku)
        {
            //AccCon.Open();
            myCon.Open();

            bool bRet = false;

            //string strSql = "Select * from 変換テーブル";
            string strSql = "Select * from ConversionTable";
            string strWhere = string.Empty;

            //strWhere += " where 人事所属 = '" + strShozok + "' ";
            strWhere += " where jimu_faculty = '" + strShozok + "' ";
            //strWhere += " and 人事係講座 = '" + strKakari + "' ";
            strWhere += " and jimu_dept = '" + strKakari + "' ";
            //strWhere += " and 人事職種 = '" + strSyoku + "' ";
            strWhere += " and jimu_occupation = '" + strSyoku + "' ";
            //strWhere += " and 全学所属 = '" + strZShozok + "' ";
            strWhere += " and ldap_faculty = '" + strZShozok + "' ";
            //strWhere += " and 全学学科 = '" + strZKakari + "' ";
            strWhere += " and ldap_dept = '" + strZKakari + "' ";
            //strWhere += " and 全学職種 = '" + strZSyoku + "' ";
            strWhere += " and ldap_occupation = '" + strZSyoku + "' ";

            //AccCom.CommandText = strSql;
            //AccCom.Connection = AccCon;
            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(strSql, myCon);
            da.Fill(dt);
            //if (AccCom.ExecuteNonQuery() == 1)
            //{
            //    bRet = true;
            //}
            if (dt.Rows.Count == 1)
            {
                bRet = true;
            }

            //AccCon.Close();
            myCon.Close();

            return bRet;

        }
        #endregion

        #region<SQL実行(削除・挿入)>
        public int SQLExecution(string strSql)
        {
            //AccCon.Open();
            myCon.Open();

            int iRet = 0;

            //AccCom.CommandText = strSql;
            //AccCom.Connection = AccCon;
            //iRet = AccCom.ExecuteNonQuery();
            MySqlCommand cmd = new MySqlCommand(strSql,myCon);
            iRet = cmd.ExecuteNonQuery();
            
            //AccCon.Close();
            myCon.Close();

            return iRet;

        }
        #endregion

        #region<SQL実行(存在確認)>
        public int SQLExecutionExistence(string strSql)
        {
            myCon.Open();

            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(strSql, myCon);
            da.Fill(dt);

            int iRet = dt.Rows.Count;

            myCon.Close();

            return iRet;

        }
        #endregion

        #region<ステータス変換テーブル取得>
        public List<string> getConvertionStatus(string strSql)
        {
            myCon.Open();

            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(strSql, myCon);
            da.Fill(dt);

            List<string> lConStat = new List<string>();

            if (dt.Rows.Count == 1)
            {
                lConStat.Add(dt.Rows[0][3].ToString().Trim()); //account_status
                lConStat.Add(dt.Rows[0][4].ToString().Trim()); //eduPersonAffiliation
            }

            myCon.Close();

            return lConStat;

        }
        #endregion

        #region<LdapAllUsr取得>
        public List<dtclsLdap> getLdapAllUsr(string strSQL)
        {
            myCon.Open();

            List<dtclsLdap> lLdapAllUsr = new List<dtclsLdap>();

            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(strSQL, myCon);
            da.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lLdapAllUsr.Add(new dtclsLdap
                {
                    ユーザID = (dt.Rows[i][0] == null ? "" : dt.Rows[i][0].ToString().Trim()),
                    氏名 = (dt.Rows[i][1] == null ? "" : dt.Rows[i][1].ToString().Trim()),
                    職員番号 = (dt.Rows[i][2] == null ? "" : dt.Rows[i][2].ToString().Trim()),
                    全学Gmail = (dt.Rows[i][3] == null ? "" : dt.Rows[i][3].ToString().Trim()),
                    アカウントステータス = (dt.Rows[i][4] == null ? "" : dt.Rows[i][4].ToString().Trim()),
                    ステータスコード = (dt.Rows[i][5] == null ? "" : dt.Rows[i][5].ToString().Trim()),
                    全学所属 = (dt.Rows[i][6] == null ? "" : dt.Rows[i][6].ToString().Trim()),
                    全学学科 = (dt.Rows[i][7] == null ? "" : dt.Rows[i][7].ToString().Trim()),
                    全学職種 = (dt.Rows[i][8] == null ? "" : dt.Rows[i][8].ToString().Trim()),
                    事務所属 = (dt.Rows[i][9] == null ? "" : dt.Rows[i][9].ToString().Trim()),
                    事務係講座 = (dt.Rows[i][10] == null ? "" : dt.Rows[i][10].ToString().Trim()),
                    事務職種 = (dt.Rows[i][11] == null ? "" : dt.Rows[i][11].ToString().Trim()),
                    事務フラグ = (dt.Rows[i][11] == null ? "" : dt.Rows[i][12].ToString().Trim()),
                    DB登録日時 = (dt.Rows[i][12] == null ? "" : dt.Rows[i][13].ToString().Trim())
                });
            }

            myCon.Close();

            return lLdapAllUsr;
        }
        #endregion

        #region<UnIssuedUsr取得>
        public List<dtclsUnIssued> getUnIssuedUsr(string strSQL)
        {
            myCon.Open();

            List<dtclsUnIssued> lUnIssuedUsr = new List<dtclsUnIssued>();

            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter(strSQL, myCon);
            da.Fill(dt);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lUnIssuedUsr.Add(new dtclsUnIssued
                {
                    職員番号 = (dt.Rows[i][0] == null ? "" : dt.Rows[i][0].ToString().Trim()),
                    氏名 = (dt.Rows[i][1] == null ? "" : dt.Rows[i][1].ToString().Trim()),
                    事務所属 = (dt.Rows[i][2] == null ? "" : dt.Rows[i][2].ToString().Trim()),
                    事務係講座 = (dt.Rows[i][3] == null ? "" : dt.Rows[i][3].ToString().Trim()),
                    事務職種 = (dt.Rows[i][4] == null ? "" : dt.Rows[i][4].ToString().Trim()),
                    DB更新日時 = (dt.Rows[i][5] == null ? "" : dt.Rows[i][5].ToString().Trim()),
                    DB登録日時 = (dt.Rows[i][6] == null ? "" : dt.Rows[i][6].ToString().Trim())
                });
            }

            myCon.Close();

            return lUnIssuedUsr;
        }
        #endregion
    }
}
