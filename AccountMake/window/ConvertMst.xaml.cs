using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.OleDb;

namespace AccountMake
{
    /// <summary>
    /// ConvertMst.xaml の相互作用ロジック
    /// </summary>
    public partial class ConvertMst : Window
    {
        #region<フィールド変数>
        DBAccess dbAObj = null;
        string strAccessPath = string.Empty; //デフォルトのAccessDBパス
        int iLastID = 0; //マスタ最終レコードID
        bool bUpdateFlg = false;
        #endregion

        #region<コンストラクタ>
        //public ConvertMst(DBAccess accobj,string strAccPath,List<string> strRecode)
        public ConvertMst(DBAccess accobj, List<string> strRecode)
        {
            InitializeComponent();

            dbAObj = accobj;

            //dbAObj.setAccConnection(strAccPath);

            if (strRecode.Count == 0)
            {
                bUpdateFlg = true;
                lblID.Text = "更新ID";
                grdRecode.Visibility = Visibility.Collapsed;
                btnInsert.Content = "更新";
                winMstMain.Height = 585;
                lblUpdata.Visibility = Visibility.Visible;
            }
            else if (strRecode[0] == "Add") // 20190521 CSV作成時以外にも追加できる機能追加
            {
                grdRecode.Visibility = Visibility.Collapsed;
                winMstMain.Height = 585;
                lblUpdata.Visibility = Visibility.Visible;
            }
            else
            {
                string strMsg = "以下の条件で所属変換できません。\n変換できるようデータを挿入してください\n";
                strMsg += "人事所属：" + strRecode[0] + "\n";
                strMsg += "人事係講座：" + strRecode[1] + "\n";
                strMsg += "人事職種：" + strRecode[2] + "\n";
                strMsg += "ユーザーID：" + strRecode[3] + "\n";
                MessageBox.Show(strMsg);
                txtSyozoku.Text = strRecode[0];
                txtKakari.Text = strRecode[1];
                txtSyoku.Text = strRecode[2];
                txtRSyozoku.Text = strRecode[0];
                txtRKakari.Text = strRecode[1];
                txtRSyoku.Text = strRecode[2];
                lblUpdata.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region<イベント>

        #region<ロードイベント>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<dtclsConvert> lConvert = dbAObj.getConvertMst(string.Empty, string.Empty, string.Empty, string.Empty,true);
                iLastID = lConvert[lConvert.Count-1].ID;
                grdDBdata.ItemsSource = lConvert;
            }
            catch (OleDbException oe)
            {
                string strErrMsg = "変換テーブル取得処理に失敗したため処理を中断します。\n ErrMsg：" + oe.Message;
                MessageBox.Show(strErrMsg);
                //dbAObj.AccCon.Close();
                dbAObj.myCon.Close();
            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
                //dbAObj.AccCon.Close();
                dbAObj.myCon.Close();
            }
        }
        #endregion

        #region<検索ボタンクリックイベント>
        private void btnSerch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                grdDBdata.ItemsSource = dbAObj.getConvertMst(txtSyozoku.Text, txtKakari.Text, txtSyoku.Text, string.Empty,true);
            }
            catch (OleDbException oe)
            {
                string strErrMsg = "変換テーブル取得処理に失敗したため処理を中断します。\n ErrMsg：" + oe.Message;
                MessageBox.Show(strErrMsg);
                //dbAObj.AccCon.Close();
                dbAObj.myCon.Close();
            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
                //dbAObj.AccCon.Close();
                dbAObj.myCon.Close();
            }
        }
        #endregion

        #region<参照IDロストフォーカスイベント>
        private void txtSansho_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                //挿入値入力用テキストボックス制御
                if(txtSansho.Text == string.Empty)
                {
                    setIntxtBoxPro(true);
                }

                int iID = 0;

                if (int.TryParse(txtSansho.Text, out iID))
                {
                    List<dtclsConvert> lConvert = new List<dtclsConvert>();

                    lConvert = dbAObj.getConvertMst(string.Empty, string.Empty, string.Empty, iID.ToString(),true);

                    txtInSyozoku.Text = lConvert[0].人事所属;
                    txtInKakari.Text = lConvert[0].人事係講座;
                    txtInSyoku.Text = lConvert[0].人事職種;
                    txtInZenSyozoku.Text = lConvert[0].全学所属;
                    txtInZenKakari.Text = lConvert[0].全学学科;
                    txtInZenSyoku.Text = lConvert[0].全学職種;

                    setIntxtBoxPro(false);
                }
                else
                {
                    MessageBox.Show("参照IDは数値を入力してください。");
                }
            }
            catch (OleDbException oe)
            {
                string strErrMsg = "変換テーブル取得処理に失敗したため処理を中断します。\n ErrMsg：" + oe.Message;
                MessageBox.Show(strErrMsg);
                //dbAObj.AccCon.Close();
                dbAObj.myCon.Close();
            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
                //dbAObj.AccCon.Close();
                dbAObj.myCon.Close();
            }
        }
        #endregion

        #region<挿入・更新ボタンクリックイベント>
        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSansho.Text != string.Empty)
                {
                    //更新処理
                    if (bUpdateFlg)
                    {
                        string strSql = string.Empty;
                        int iResult = 0;

                        //何も入力されていない場合は削除
                        if (txtInSyozoku.Text == string.Empty &&
                           txtInKakari.Text == string.Empty &&
                           txtInSyoku.Text == string.Empty &&
                           txtInZenSyozoku.Text == string.Empty &&
                           txtInZenKakari.Text == string.Empty &&
                           txtInZenSyoku.Text == string.Empty)
                        {
                            MessageBoxResult mResult = MessageBox.Show("値が入力されていません。\nID:" + txtSansho.Text + " を削除しますか？", "", MessageBoxButton.YesNo);

                            if (mResult == MessageBoxResult.Yes)
                            {
                                //strSql = "delete from 変換テーブル where ID = " + txtSansho.Text;
                                strSql = "delete from ConversionTable where id = " + txtSansho.Text;

                                //iResult = dbAObj.InsertConvertMst(strSql); SQL実行処理統一化のためコメントアウト
                                iResult = dbAObj.SQLExecution(strSql);

                                MessageBox.Show("データの削除に成功しました。\n削除データ数" + iResult.ToString() + "件");

                                //画面更新
                                List<dtclsConvert> lConvert = dbAObj.getConvertMst(string.Empty, string.Empty, string.Empty, string.Empty, true);
                                iLastID = lConvert[lConvert.Count-1].ID;
                                grdDBdata.ItemsSource = lConvert;
                            }
                        }
                        //更新処理
                        else
                        {
                            MessageBoxResult mResult = MessageBox.Show("入力された値でID:" + txtSansho.Text + " を更新しますか？", "", MessageBoxButton.YesNo);

                            if (mResult == MessageBoxResult.Yes)
                            {
                                //strSql = "update 変換テーブル set ";
                                strSql = "update ConversionTable set ";

                                if (txtInSyozoku.Text == string.Empty)
                                {
                                    //strSql += "変換テーブル.人事所属 = Null ,";
                                    strSql += "jimu_faculty = Null ,";
                                }
                                else
                                {
                                    //strSql += "変換テーブル.人事所属 = '" + txtInSyozoku.Text + "' , ";
                                    strSql += "jimu_faculty = '" + txtInSyozoku.Text + "' , ";
                                }

                                if (txtInKakari.Text == string.Empty)
                                {
                                    //strSql += "変換テーブル.人事係講座 = Null ,";
                                    strSql += "jimu_dept = Null ,";
                                }
                                else
                                {
                                    //strSql += "変換テーブル.人事係講座 = '" + txtInKakari.Text + "' , ";
                                    strSql += "jimu_dept = '" + txtInKakari.Text + "' , ";
                                }

                                if (txtInSyoku.Text == string.Empty)
                                {
                                    //strSql += "変換テーブル.人事職種 = Null ,";
                                    strSql += "jimu_occupation = Null ,";
                                }
                                else
                                {
                                    //strSql += "変換テーブル.人事職種 = '" + txtInSyoku.Text + "' , ";
                                    strSql += "jimu_occupation = '" + txtInSyoku.Text + "' , ";
                                }

                                if (txtInZenSyozoku.Text == string.Empty)
                                {
                                    //strSql += "変換テーブル.全学所属 = Null ,";
                                    strSql += "ldap_faculty = Null ,";
                                }
                                else
                                {
                                    //strSql += "変換テーブル.全学所属 = '" + txtInZenSyozoku.Text + "' , ";
                                    strSql += "ldap_faculty = '" + txtInZenSyozoku.Text + "' , ";
                                }

                                if (txtInZenKakari.Text == string.Empty)
                                {
                                    //strSql += "変換テーブル.全学学科 = Null ,";
                                    strSql += "ldap_dept = Null ,";
                                }
                                else
                                {
                                    //strSql += "変換テーブル.全学学科 = '" + txtInZenKakari.Text + "' , ";
                                    strSql += "ldap_dept = '" + txtInZenKakari.Text + "' , ";
                                }

                                if (txtInZenSyoku.Text == string.Empty)
                                {
                                    //strSql += "変換テーブル.全学職種 = Null ,";
                                    strSql += "ldap_occupation = Null ,";
                                }
                                else
                                {
                                    //strSql += "変換テーブル.全学職種 = '" + txtInZenSyoku.Text + "' ";
                                    strSql += "ldap_occupation = '" + txtInZenSyoku.Text + "' ";
                                }

                                //strSql += "where ID = " + txtSansho.Text;
                                strSql += "where id = " + txtSansho.Text;

                                //iResult = dbAObj.InsertConvertMst(strSql); SQL実行処理統一化のためコメントアウト
                                iResult = dbAObj.SQLExecution(strSql);

                                MessageBox.Show("データの更新に成功しました。\n更新データ数" + iResult.ToString() + "件");

                                //画面更新
                                List<dtclsConvert> lConvert = dbAObj.getConvertMst(string.Empty, string.Empty, string.Empty, string.Empty, true);
                                iLastID = lConvert[lConvert.Count - 1].ID;
                                grdDBdata.ItemsSource = lConvert;
                            }
                        }
                    }
                    //挿入処理
                    else
                    {
                        if (txtInSyozoku.Text == string.Empty)
                        {
                            MessageBox.Show("人事所属が入力されていません。\n処理を中断します。");
                            return;
                        }

                        if (txtInSyoku.Text == string.Empty)
                        {
                            MessageBox.Show("人事職種が入力されていません。\n処理を中断します。");
                            return;
                        }

                        if (txtInZenSyozoku.Text == string.Empty)
                        {
                            MessageBox.Show("全学所属が入力されていません。\n処理を中断します。");
                            return;
                        }

                        if (txtInZenSyoku.Text == string.Empty)
                        {
                            MessageBox.Show("全学職種が入力されていません。\n処理を中断します。");
                            return;
                        }

                        if (dbAObj.chkConvertMst(txtInSyozoku.Text.Trim(), txtInKakari.Text.Trim(), txtInSyoku.Text.Trim(), txtInZenSyozoku.Text.Trim(), txtInZenKakari.Text.Trim(), txtInZenSyoku.Text.Trim()))
                        {
                            MessageBox.Show("同様の内容がすでに入力されています。\n処理を中断します。");
                            return;
                        }

                        MessageBoxResult mResult = MessageBox.Show("入力された値で変換テーブルに入力します。\nよろしいですか？", "", MessageBoxButton.YesNo);

                        if (mResult == MessageBoxResult.No)
                        {
                            MessageBox.Show("データの挿入を中断しました。");

                            dbAObj.myCon.Close();
                            Close();

                            return;
                        }

                        //string strInsertSql = "Insert into 変換テーブル ";
                        string strInsertSql = "Insert into ConversionTable ";
                        string strSelectSql = "Select ";
                        int iResult = 0;

                        //strSelectSql += (iLastID + 1) + " as ID,";
                        strSelectSql += (iLastID + 1) + " as id,";
                        //strSelectSql += "'" + txtInSyozoku.Text.Trim() + "' as 人事所属,";
                        strSelectSql += "'" + txtInSyozoku.Text.Trim() + "' as jimu_faculty,";

                        //空欄の場合はNullを挿入(mdb作成時の元データがNullのため)
                        if (txtInKakari.Text.Trim() == string.Empty)
                        {
                            //strSelectSql += "Null as 人事係講座,";
                            strSelectSql += "Null as jimu_dept,";
                        }
                        else
                        {
                            //strSelectSql += "'" + txtInKakari.Text.Trim() + "' as 人事係講座,";
                            strSelectSql += "'" + txtInKakari.Text.Trim() + "' as jimu_dept,";
                        }

                        //strSelectSql += "'" + txtInSyoku.Text.Trim() + "' as 人事職種,";
                        strSelectSql += "'" + txtInSyoku.Text.Trim() + "' as jimu_occupation,";
                        //strSelectSql += "'" + txtInZenSyozoku.Text.Trim() + "' as 全学所属,";
                        strSelectSql += "'" + txtInZenSyozoku.Text.Trim() + "' as ldap_faculty,";
                        //strSelectSql += "'" + txtInZenKakari.Text.Trim() + "' as 全学学科,";
                        strSelectSql += "'" + txtInZenKakari.Text.Trim() + "' as ldap_dept,";
                        //strSelectSql += "'" + txtInZenSyoku.Text.Trim() + "' as 全学職種,";
                        strSelectSql += "'" + txtInZenSyoku.Text.Trim() + "' as ldap_occupation,";
                        //strSelectSql += "変換テーブル.AA荒牧D,";
                        strSelectSql += "adapt_dir_ara,";
                        //strSelectSql += "変換テーブル.AA荒牧部,";
                        strSelectSql += "adapt_dep_ara,";
                        //strSelectSql += "変換テーブル.AA昭和D,";
                        strSelectSql += "adapt_dir_sho,";
                        //strSelectSql += "変換テーブル.AA昭和部,";
                        strSelectSql += "adapt_dep_sho,";
                        //strSelectSql += "変換テーブル.AA桐生D,";
                        strSelectSql += "adapt_dir_kir,";
                        //strSelectSql += "変換テーブル.AA桐生部 ";
                        strSelectSql += "adapt_dep_kir ";
                        //strSelectSql += "from 変換テーブル where ID = " + txtSansho.Text.Trim();
                        strSelectSql += "from ConversionTable where id = " + txtSansho.Text.Trim();

                        strInsertSql += strSelectSql;

                        //iResult = dbAObj.InsertConvertMst(strInsertSql);
                        iResult = dbAObj.SQLExecution(strInsertSql);

                        MessageBox.Show("データの挿入に成功しました。\n変換テーブル操作画面を終了します。\n挿入データ数" + iResult.ToString() + "件");

                        dbAObj.myCon.Close();
                        Close();
                    }
                }
                else
                {
                    if(bUpdateFlg)
                    {
                        MessageBox.Show("更新処理は更新ID入力後に実行可能です。");
                    }
                    else
                    {
                        MessageBox.Show("挿入処理は参照ID入力後に実行可能です。");
                    }
                }
            }
            catch (OleDbException oe)
            {
                string strErrMsg = "変換テーブル更新処理に失敗したため処理を中断します。\n ErrMsg：" + oe.Message;
                MessageBox.Show(strErrMsg);
                //dbAObj.AccCon.Close();
                dbAObj.myCon.Close();
            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
                //dbAObj.AccCon.Close();
                dbAObj.myCon.Close();
            }
        }
        #endregion

        #endregion

        #region<メソッド>

        #region<挿入テキストボックスコントロールメソッド>
        private void setIntxtBoxPro(bool bValue)
        {
            txtInSyozoku.IsReadOnly = bValue;
            txtInKakari.IsReadOnly = bValue;
            txtInSyoku.IsReadOnly = bValue;
            txtInZenSyozoku.IsReadOnly = bValue;
            txtInZenKakari.IsReadOnly = bValue;
            txtInZenSyoku.IsReadOnly = bValue;
        }
        #endregion

        #endregion
    }
}
