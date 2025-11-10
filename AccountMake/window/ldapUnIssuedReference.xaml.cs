using System;
using System.Windows;

using MySql.Data.MySqlClient;

namespace AccountMake
{
    /// <summary>
    /// ConvertMst.xaml の相互作用ロジック
    /// </summary>
    public partial class Reference : Window
    {
        #region<フィールド変数>
        DBAccess dbAObj = null;
        #endregion

        #region<コンストラクタ>
        //public ConvertMst(DBAccess accobj,string strAccPath,List<string> strRecode)
        public Reference(DBAccess dbaob)
        {
            InitializeComponent();

            dbAObj = dbaob;
        }
        #endregion

        #region<イベント>

        #region<ロードイベント>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //未申請者一覧取得
                grdUnIssueddata.ItemsSource = dbAObj.getUnIssuedUsr("select A.id_number,A.name,B.faculty,B.dept,B.occupation,A.up_time,A.ins_time " +
                    "from UnIssuedUsr as A Inner join AllTargetUsers as B on A.id_number = B.id_number");

                //全学認証一覧取得
                grdLdapdata.ItemsSource = dbAObj.getLdapAllUsr("select * from LdapAllUsr");
            }
            catch (MySqlException me)
            {
                string strErrMsg = "DBデータ取得処理に失敗したため処理を中断します。\n ErrMsg：" + me.Message;
                MessageBox.Show(strErrMsg);
                dbAObj.myCon.Close();
            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
                dbAObj.myCon.Close();
            }

        }

        #endregion

        #endregion

        #region<未申請者一覧検索ボタンクリックイベント>
        private void btnUnIssedSerch_Click(object sender, RoutedEventArgs e)
        {
            string strSql = "select A.id_number,A.name,B.faculty,B.dept,B.occupation,A.up_time,A.ins_time " +
                            "from UnIssuedUsr as A Inner join AllTargetUsers as B on A.id_number = B.id_number ";
            string strWhere = string.Empty;

            //職員番号
            if (txtUnIssuedjimuid.Text != string.Empty)
            {
                strWhere += " where A.id_number = '" + txtUnIssuedjimuid.Text.Trim() + "' ";
            }

            //氏名
            if (txtUnIssuedname.Text != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where A.name like '%" + txtUnIssuedname.Text.Trim() + "%' ";
                }
                else
                {
                    strWhere += " and A.name like '%" + txtUnIssuedname.Text.Trim() + "%' ";
                }
            }

            //事務所属
            if (txtUnIssuedfaculty.Text != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where B.faculty like '%" + txtUnIssuedfaculty.Text.Trim() + "%' ";
                }
                else
                {
                    strWhere += " and B.faculty like '%" + txtUnIssuedfaculty.Text.Trim() + "%' ";
                }
            }

            if (strWhere != string.Empty)
            {
                strSql += strWhere;
            }

            grdUnIssueddata.ItemsSource = dbAObj.getUnIssuedUsr(strSql);
        }
        #endregion

        #region<全学アカウント一覧検索ボタンクリックイベント>
        private void btnLdapSearch_Click(object sender, RoutedEventArgs e)
        {
            string strSql = "select * from LdapAllUsr ";
            string strWhere = string.Empty;

            //ユーザID
            if (txtldapid.Text != string.Empty)
            {
                strWhere += " where ldap_id = '" + txtldapid.Text.Trim() + "' ";
            }

            //氏名
            if (txtldapname.Text != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where user_name like '%" + txtldapname.Text.Trim() + "%' ";
                }
                else
                {
                    strWhere += " and user_name like '%" + txtldapname.Text.Trim() + "%' ";
                }
            }

            //職員番号
            if (txtldapjimuid.Text != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where jimu_id = '" + txtldapjimuid.Text.Trim() + "' ";
                }
                else
                {
                    strWhere += " and jimu_id = '" + txtldapjimuid.Text.Trim() + "' ";
                }
            }

            //全学所属
            if (txtldapfaculty.Text != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where ldap_faculty like '%" + txtldapfaculty.Text.Trim() + "%' ";
                }
                else
                {
                    strWhere += " and ldap_faculty like '%" + txtldapfaculty.Text.Trim() + "%' ";
                }
            }

            //事務所属
            if (txtjimufaclty.Text != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where jimu_faculty like '%" + txtjimufaclty.Text.Trim() + "%' ";
                }
                else
                {
                    strWhere += " and jimu_faculty like '%" + txtjimufaclty.Text.Trim() + "%' ";
                }
            }

            //全学職種
            if (txtldapoccupation.Text != string.Empty)
            {
                if (strWhere == string.Empty)
                {
                    strWhere += " where ldap_occupation like '%" + txtldapoccupation.Text.Trim() + "%' ";
                }
                else
                {
                    strWhere += " and ldap_occupation like '%" + txtldapoccupation.Text.Trim() + "%' ";
                }
            }

            if (strWhere != string.Empty)
            {
                strSql += strWhere;
            }

            grdLdapdata.ItemsSource = dbAObj.getLdapAllUsr(strSql);
        }
        #endregion
    }
}
