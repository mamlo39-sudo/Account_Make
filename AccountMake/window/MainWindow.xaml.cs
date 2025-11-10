//全学アカウント作成支援アプリ
//作成日 2017/5/9
//作成者　綿貫
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using MySql.Data.MySqlClient;
using System.IO;
using Microsoft.Win32;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AccountMake
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region<フィールド変数>
        DBAccess dbAObj = null;
        List<dtclsRequest> lRequest = new List<dtclsRequest>(); //requestの中身保持
        List<dtclsCSV> lCSV = new List<dtclsCSV>(); //登録用CSVの中身保持
        List<string> lBat = new List<string>(); //復帰ユーザメール送付バッチ保持
        //string strAccessDBPath = Environment.CurrentDirectory + "\\全学変換テーブル.mdb"; //デフォルト変換テーブルパス
        static string strAppUsr = ConfigurationManager.ConnectionStrings["ApplicationUser"].ToString().Trim(); //アプリ使用者
        static string strSetTable = ConfigurationManager.ConnectionStrings["SetTable"].ToString().Trim(); //初期設定テーブル
        #endregion

        #region<コンストラクタ>
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region<イベント>

        #region<ロードイベント>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //基盤係が使用するときか参照モードのときは変換テーブルを修正できないようにする
                if(strAppUsr == "jimu" || strAppUsr == "Reference")
                {
                    //変換テーブル更新ボタン使用不可
                    btnMst.IsEnabled = false;

                    //変換テーブル追加ボタン使用不可
                    btnMstAdd.IsEnabled = false;

                    //事務職員表示有無チェックボタンを非表示
                    chkDevFlg.Visibility = Visibility.Collapsed;

                    //接続先テーブル選択は変更不可
                    cmbTableList.IsEnabled = false;
                }

                //参照モードのときはCSV作成ボタン，対象選択ボタン等を使用不可にする
                if (strAppUsr == "Reference")
                {
                    //対象選択ボタン使用不可
                    btnSelect.IsEnabled = false;
                    //CSV作成ボタン使用不可
                    btnCsvListMake.IsEnabled = false;

                    //事務職員表示有無チェックボタンを非表示
                    chkDevFlg.Visibility = Visibility.Collapsed;

                    //画面サイズの変更
                    winMain.Height = 580;
                    LayoutRoot.Height = 580;
                }

                // CSV作成オプションは検索対象がrequestテーブルに変更された時だけ表示する。
                BorCsvMake.SetValue(Grid.ColumnSpanProperty, 8);
                chkexceptExitFlg.Visibility = Visibility.Collapsed;
                chkexceptReFlg.Visibility = Visibility.Collapsed;

                //変換テーブル用接続文字列作成 変換テーブルMySQL化により削除
                //if (!File.Exists(strAccessDBPath))
                //{
                //    MessageBox.Show("変換テーブル用Accessファイルが見つかりません。\n読み込むAccessファイルを選択してください。");

                //    //パス取得用ダイアログ表示
                //    OpenFileDialog ofd = new OpenFileDialog();
                //    ofd.FilterIndex = 1;
                //    ofd.Filter = "Accessファイル(.mdb)|*.mdb";
                //    bool bResult = (bool)ofd.ShowDialog();

                //    if (bResult)
                //    {
                //        MessageBoxResult mResult = MessageBox.Show("選択された変換テーブルを今後も参照しますか？", "", MessageBoxButton.YesNo);

                //        if (mResult == MessageBoxResult.Yes)
                //        {
                //            File.Copy(ofd.FileName, strAccessDBPath, true);
                //        }

                //        strAccessDBPath = ofd.FileName;
                //    }
                //    else
                //    {
                //        MessageBox.Show("ファイルが選択されませんでした。\n処理を中断します。");

                //        return;
                //    }
                //}

                dbAObj = new DBAccess();

                List<string> lstrTable = dbAObj.getTableList();
                int iRequestNo = 0;

                //コンボボックスにテーブル一覧を設定
                cmbTableList.ItemsSource = lstrTable;

                //初期設定テーブルの要素番号を取得
                for(iRequestNo = 0; iRequestNo < lstrTable.Count; iRequestNo++)
                {
                    //if(lstrTable[iRequestNo] == "request")
                    if (lstrTable[iRequestNo] == strSetTable)
                    {
                        break;
                    }
                }

                //コンボボックスを申請テーブルに設定
                cmbTableList.SelectedIndex = iRequestNo;

                //コンボボックスで選択されたテーブルの一覧を取得
                getDBData();
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

        #region<テーブルリストコンボ変更イベント>
        private void cmbTableList_DropDownClosed(object sender, EventArgs e)
        {
            try
            {
                getDBData();

                // 参照以外のユーザの場合
                if (strAppUsr != "Reference")
                {

                    if ((string)cmbTableList.SelectedValue == "request" || (string)cmbTableList.SelectedValue == "UnIssuedRecode")
                    {
                        grdSerchAction.Visibility = Visibility.Visible;
                        winMain.Height = 1000;
                        LayoutRoot.Height = 1000;

                        // requestを参照する場合はCSV作成時に細かい指定ができるチェックボックスを表示
                        if ((string)cmbTableList.SelectedValue == "request")
                        {
                            BorCsvMake.SetValue(Grid.ColumnSpanProperty,9);
                            chkexceptExitFlg.Visibility = Visibility.Visible;
                            chkexceptReFlg.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            BorCsvMake.SetValue(Grid.ColumnSpanProperty, 8);
                            chkexceptExitFlg.Visibility = Visibility.Collapsed;
                            chkexceptReFlg.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        grdSerchAction.Visibility = Visibility.Collapsed;
                        winMain.Height = 450;
                        LayoutRoot.Height = 450;
                    }
                }
            }
            catch (MySqlException me)
            {
                string strErrMsg = "テーブル一覧取得処理に失敗したため処理を中断します。\n ErrMsg：" + me.Message;
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

        #region<データ再取得ボタンクリックイベント>
        private void btnReflash_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btnName = (Button)sender;

                if (btnName.Name == "btnReflash")
                {
                    txtUsr.Text = string.Empty;
                    txtNo.Text = string.Empty;
                    txtSei.Text = string.Empty;
                    txtMei.Text = string.Empty;
                    txtFaculty.Text = string.Empty;
                    txtNaisen.Text = string.Empty;
                }

                getDBData();
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

        #region<選択ボタンクリックイベント>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            int iStartNo = 0;
            int iEndNo = 0;

            try
            {
                if (!int.TryParse(txtselectstart.Text, out iStartNo) || !int.TryParse(txtselectend.Text, out iEndNo))
                {
                    MessageBox.Show("数値以外の値が入力されています。\n処理を中断しました。");
                }
                else if (iEndNo < iStartNo)
                {
                    MessageBox.Show("終了番号に開始番号より小さい数値が入力されています。\n処理を中断しました。");
                }
                else
                {
                    SetSelect(iStartNo, iEndNo);
                }
            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
            }
        }
        #endregion

        #region<グリッドクリックイベント>
        private void grdDBdata_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string strCmbValue = (string)cmbTableList.SelectedValue;

            try
            {
                if (strCmbValue == "request" || strCmbValue == "UnIssuedRecode")
                {
                    int iSelectRow = grdDBdata.SelectedIndex;

                    SetSelect(iSelectRow, iSelectRow);
                }
            }
            catch(Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
            }
        }
        #endregion

        #region<所属変換テーブル更新ボタンクリックイベント>
        private void btnMst_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> lEmpty = new List<string>();

                OpenMst(lEmpty);
            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
            }
        }
        #endregion

        #region<所属変換テーブル追加ボタンクリックイベント>
        private void btnMstAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> lEmpty = new List<string>();

                lEmpty.Add("Add");

                OpenMst(lEmpty);
            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
            }
        }
        #endregion

        #region<CSV作成ボタンクリックイベント>
        private void btnCsvListMake_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lCSV = new List<dtclsCSV>();
                lBat = new List<string>();
                List<string> lConStat = new List<string>();
                List<dtclsAA> lAA = new List<dtclsAA>();

                foreach (dtclsRequest tmpRequest in lRequest)
                {
                    int iChkCount = 0;
                    int iResult = 0;
                    string strSQL = string.Empty;
                    string chkSql = string.Empty;
                    string strlmployment = string.Empty;
                    // 事務登録担当が出力する場合は職種は固定にするため変数を用意しておく
                    string strjimufaculty = "事務局";
                    string strjimuoccupation = string.Empty;
                    if (tmpRequest.職員番号.Length > 2)
                    {
                        strlmployment = tmpRequest.職員番号.Substring(0, 2) == "16" ? "非常勤" : "常勤";
                        strjimuoccupation = tmpRequest.職員番号.Substring(0, 2) == "16" ? "事務補佐員" : "事務職員";
                    }
                    bool bExistFlg = false; // 処理除外フラグ
                    bool bAAonlyFlg = false; // アカウントアダプターのデータのみ作成するフラグ

                    if (tmpRequest.処理対象)
                    {
                        List<dtclsConvert> tmpConvert = null;

                        // 所属変換処理
                        // request選択時，再申請除外で再申請データの場合は所属変換処理は行わない
                        if (!((bool)chkexceptReFlg.IsChecked && tmpRequest.再申請 == "TRUE"))
                        {
                            // 事務職員は所属：事務局，職種：事務職員 or 事務補佐員で固定なので所属変換しない
                            if (strAppUsr != "jimu")
                            {
                                while (true)
                                {
                                    //dbAObj.setAccConnection(strAccessDBPath);
                                    tmpConvert = dbAObj.getConvertMst(tmpRequest.所属, tmpRequest.係講座等, tmpRequest.職種, string.Empty, false);

                                    if (tmpConvert.Count == 1)
                                    {
                                        break;
                                    }
                                    else if (tmpConvert.Count >= 2 && (string)cmbTableList.SelectedValue != "request") // requestテーブル選択時はチェック処理は未実施にする
                                    {
                                        //string strMsg = "以下の条件が重複して登録されています。\nアプリ配置フォルダ内の全額変換テーブル.mdbを開き，データを修正してください。\n";
                                        string strMsg = "以下の条件が重複して登録されています。\n変換マスタのデータを修正してください。\n";
                                        strMsg += "人事所属：" + tmpRequest.所属 + "\n";
                                        strMsg += "人事係講座：" + tmpRequest.係講座等 + "\n";
                                        strMsg += "人事職種：" + tmpRequest.職種;
                                        MessageBox.Show(strMsg);

                                        return;
                                    }
                                    else if (tmpConvert.Count == 0 && iChkCount == 0)
                                    {
                                        // 基盤係が登録の時は処理を中断
                                        if (strAppUsr == "jimu")
                                        {
                                            string strMsg = "以下の条件で所属変換できないため，\nCSVは作成できません。\n\n";
                                            strMsg += "申請番号：" + tmpRequest.申請番号 + "\n";
                                            strMsg += "人事所属：" + tmpRequest.所属 + "\n";
                                            strMsg += "人事係講座：" + tmpRequest.係講座等 + "\n";
                                            strMsg += "人事職種：" + tmpRequest.職種 + "\n";
                                            strMsg += "ユーザーID：" + tmpRequest.ユーザID + "\n";
                                            MessageBox.Show(strMsg);

                                            return;
                                        }
                                        else
                                        {
                                            List<string> lstrDB = new List<string>();
                                            lstrDB.Add(tmpRequest.所属);
                                            lstrDB.Add(tmpRequest.係講座等);
                                            lstrDB.Add(tmpRequest.職種);
                                            lstrDB.Add(tmpRequest.ユーザID);
                                            OpenMst(lstrDB);
                                            iChkCount++;
                                        }
                                    }
                                    //1回マスタ開いてその後もデータがないようだったら継続確認
                                    else if (tmpConvert.Count == 0 && iChkCount == 1)
                                    {
                                        MessageBoxResult mResult = MessageBox.Show("変換テーブルに変換可能なデータが挿入されませんでした。\nCSV作成処理を継続しますか？", "", MessageBoxButton.YesNo);

                                        if (mResult == MessageBoxResult.Yes)
                                        {
                                            iChkCount = 0;
                                        }
                                        else
                                        {
                                            MessageBox.Show("CSV作成処理を終了します。");
                                            return;
                                        }
                                    }
                                }
                            }

                            // 学外研究員でないか確認する。　→　学外研究員の場合，ステータス変換は学外研究員職種マスタで行う
                            strSQL = "select * from specialuse_id_number where id_number = '" + tmpRequest.職員番号 + "'";
                            bool chkSpesiauser = (dbAObj.SQLExecutionExistence(strSQL) > 0);
                            if (chkSpesiauser)
                            {
                                strSQL = "select * from SpecialuserMst where jimu_occupation = '" + tmpRequest.職種 + "'";
                            }
                            else
                            {
                                //アカウントステータス取得
                                if (strAppUsr != "jimu")
                                {
                                    strSQL = "select * from ConversionStatus where ldap_occupation = '" + tmpConvert[0].全学職種 + "' and lmployment_status = '" + strlmployment + "'";
                                }
                                //事務職員は職種を事務職員 or 事務補佐員でステータス変換する。
                                else
                                {
                                    strSQL = "select * from ConversionStatus where ldap_occupation = '" + strjimuoccupation + "' and lmployment_status = '" + strlmployment + "'";
                                }
                            }

                            lConStat = dbAObj.getConvertionStatus(strSQL);

                            if (lConStat.Count != 2)
                            {
                                string strMsg = string.Empty;

                                // 学外研究員かそうでないかでメッセージを変更する。
                                if (chkSpesiauser)
                                {
                                    strMsg = "以下の条件でステータス変換できませんでした。\nCSV作成処理を中断します。\n\n";
                                    strMsg += "学外研究員職種マスタにない職種です。\nアカウント発行の許可された職種か確認してください。\n\n";
                                    strMsg += "許可された職種の場合は学外研究員職種マスタにデータを追加してください。\n\n";
                                    strMsg += "申請番号：" + tmpRequest.申請番号 + "\n";
                                    strMsg += "ユーザID：" + tmpRequest.ユーザID + "\n";
                                    strMsg += "職員番号：" + tmpRequest.職員番号 + "\n";
                                    strMsg += "氏名：" + tmpRequest.姓_漢字 + "　" + tmpRequest.名_漢字 + "\n";
                                    strMsg += "職種：" + tmpRequest.職種 + "\n";

                                }
                                else
                                {
                                    strMsg = "以下の条件でステータス変換できませんでした。\nCSV作成処理を中断します。\n\n所属変換マスタの登録内容に誤りがある可能性があります。";
                                    strMsg += "申請番号：" + tmpRequest.申請番号 + "\n";
                                    strMsg += "ユーザID：" + tmpRequest.ユーザID + "\n";
                                    strMsg += "職員番号：" + tmpRequest.職員番号 + "\n";
                                    strMsg += "氏名：" + tmpRequest.姓_漢字 + "　" + tmpRequest.名_漢字 + "\n";
                                    strMsg += "全学職種：" + tmpConvert[0].全学職種 + "\n";
                                    strMsg += "雇用形態：" + strlmployment + "\n\n";
                                }
                                MessageBox.Show(strMsg);
                                return;
                            }

                        }

                        #region<エラーチェック処理>

                        // requestテーブル選択時はデータ確認時のため，チェック処理を実行しない
                        if ((string)cmbTableList.SelectedValue != "request")
                        {

                            //職員番号重複チェック
                            chkSql = "select * from LdapAllUsr where jimu_id = '" + tmpRequest.職員番号 + "' and ldap_id != '" + tmpRequest.ユーザID + "' and status_code <> '13'";
                            iResult = dbAObj.SQLExecutionExistence(chkSql);
                            if (iResult >= 1)
                            {
                                string strMsg = "下記の申請者の職員番号はすでに登録されています。\nアカウントが重複するため作成しないでください。\n\n";
                                strMsg += "申請番号：" + tmpRequest.申請番号 + "\n";
                                strMsg += "ユーザID：" + tmpRequest.ユーザID + "\n";
                                strMsg += "職員番号：" + tmpRequest.職員番号 + "\n";
                                strMsg += "氏名：" + tmpRequest.姓_漢字 + "　" + tmpRequest.名_漢字 + "\n";
                                MessageBox.Show(strMsg);
                                return;
                            }

                            //ID重複チェック
                            chkSql = "select * from LdapAllUsr where ldap_id = '" + tmpRequest.ユーザID + "' and status_code <> '13'";
                            iResult = dbAObj.SQLExecutionExistence(chkSql);
                            if (iResult >= 1)
                            {
                                string strMsg = "下記の申請者のアカウントIDはすでに登録されています。\nアカウントIDを変更してください。\n\n";
                                strMsg += "申請番号：" + tmpRequest.申請番号 + "\n";
                                strMsg += "ユーザID：" + tmpRequest.ユーザID + "\n";
                                strMsg += "職員番号：" + tmpRequest.職員番号 + "\n";
                                strMsg += "氏名：" + tmpRequest.姓_漢字 + "　" + tmpRequest.名_漢字 + "\n";
                                MessageBox.Show(strMsg);
                                return;
                            }

                            //過去在籍チェック
                            chkSql = "select * from LdapAllUsr where ldap_id = '" + tmpRequest.ユーザID + "' and status_code = '13' and user_name like '%" + tmpRequest.名_漢字 + "%'";
                            iResult = dbAObj.SQLExecutionExistence(chkSql);
                            if (iResult >= 1)
                            {
                                // わかりやすいようにアカウントステータスの名称も表示する
                                string strStatName = string.Empty;

                                switch (lConStat[0].Trim())
                                {
                                    case "11":
                                        strStatName = "常勤教員";
                                        break;
                                    case "21":
                                        strStatName = "非常勤教員";
                                        break;
                                    case "31":
                                        strStatName = "常勤職員";
                                        break;
                                    case "41":
                                        strStatName = "非常勤職員";
                                        break;
                                    case "51":
                                        strStatName = "名誉教授";
                                        break;
                                    case "61":
                                        strStatName = "特別研究員";
                                        break;
                                    case "71":
                                        strStatName = "共同研究員";
                                        break;

                                }

                                // 事務は所属変換しないので固定値をメッセージに設定する。
                                string strMsg = string.Empty;
                                if (strAppUsr != "jimu")
                                {
                                    strMsg = "申請No:" + tmpRequest.申請番号 + ",ユーザID：" + tmpRequest.ユーザID + "は過去に登録されています。\n登録情報を以下に変更してください。\n\n";
                                    strMsg += "アカウントステータス：" + lConStat[0].Trim() + ":" + strStatName + "\n";
                                    strMsg += "職員番号：" + tmpRequest.職員番号 + "\n";
                                    strMsg += "内線番号：" + tmpRequest.内線番号 + "\n";
                                    strMsg += "所属：" + tmpConvert[0].全学所属 + "\n";
                                    strMsg += "学科：" + tmpConvert[0].全学学科 + "\n";
                                    strMsg += "職名：" + tmpConvert[0].全学職種 + "\n";
                                    strMsg += "人事所属：" + tmpConvert[0].人事係講座 + "\n";
                                    strMsg += "人事部局：" + tmpConvert[0].人事所属 + "\n";
                                    strMsg += "人事職種：" + tmpConvert[0].人事職種 + "\n";
                                    strMsg += "雇用形態：" + strlmployment + "\n";
                                    strMsg += "パスワード：" + tmpRequest.パスワード + "\n\n";
                                    strMsg += "※パスワードはクリップボートに出力されるので,貼り付けできます。";
                                }
                                else
                                {
                                    strMsg = "申請No:" + tmpRequest.申請番号 + ",ユーザID：" + tmpRequest.ユーザID + "は過去に登録されています。\n登録情報を以下に変更してください。\n\n";
                                    strMsg += "アカウントステータス：" + lConStat[0].Trim() + ":" + strStatName + "\n";
                                    strMsg += "職員番号：" + tmpRequest.職員番号 + "\n";
                                    strMsg += "所属：" + strjimufaculty + "\n";
                                    strMsg += "学科：" + "\n";
                                    strMsg += "職名：" + strjimuoccupation + "\n";
                                    strMsg += "人事所属：" + tmpRequest.所属 + "\n";
                                    strMsg += "人事部局：" + tmpRequest.係講座等 + "\n";
                                    strMsg += "人事職種：" + tmpRequest.職種 + "\n";
                                    strMsg += "雇用形態：" + strlmployment + "\n";
                                    strMsg += "パスワード：" + tmpRequest.パスワード + "\n\n";
                                    strMsg += "※パスワードはクリップボートに出力されるので,貼り付けできます。";
                                }

                                Clipboard.SetText(tmpRequest.パスワード);
                                MessageBox.Show(strMsg);

                                lBat.Add("call C:\\scripts\\mailsender.bat " + tmpRequest.連絡先 + " \"" + tmpRequest.姓_漢字 + " " + tmpRequest.名_漢字 +
                                    "\" " + tmpRequest.ユーザID + " password " + tmpRequest.ユーザID + "@gunma-u.ac.jp" + " \"" + tmpRequest.姓_英字 + " " + tmpRequest.名_英字 + "\" " + tmpRequest.職員番号 + " " + tmpRequest.内線番号);

                                // bExistFlg = true; 20190408 再申請の場合も機器登録用のアカウントがあるか確認したいので除外フラグは立てないようにする。
                                bAAonlyFlg = true;
                            }

                            //再申請時ID確認すり抜けチェック(職員番号が変わって過去存在した別の人のIDを入力した場合)
                            if (tmpRequest.再申請 == "TRUE")
                            {
                                List<dtclsRequest> chkRequest = new List<dtclsRequest>();
                                string strGetTable = "request";

                                chkRequest = dbAObj.getRequest(tmpRequest.ユーザID, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, strGetTable, (bool)chkDevFlg.IsChecked);

                                //取得したデータすべてチェック
                                for (int i = 0; i < chkRequest.Count; i++)
                                {
                                    //名前(漢字)と職員番号が一致しない場合は処理を中断(小文字に統一して比較)
                                    if (chkRequest[i].名_漢字.ToLower() != tmpRequest.名_漢字.ToLower() && chkRequest[i].職員番号 != tmpRequest.職員番号)
                                    {
                                        //姓名を逆にして一致するときは処理を継続させる
                                        if (chkRequest[i].名_漢字.ToLower() != tmpRequest.姓_漢字.ToLower() && chkRequest[i].名_漢字.ToLower() != tmpRequest.姓_漢字.ToLower())
                                        {

                                            string strMsg = "下記の申請者が別の人のアカウントIDで申請しています。\nアカウントIDを変更してください。\n\n";
                                            strMsg += "申請番号：" + tmpRequest.申請番号 + "\n";
                                            strMsg += "ユーザID：" + tmpRequest.ユーザID + "\n";
                                            strMsg += "職員番号：" + tmpRequest.職員番号 + "\n";
                                            strMsg += "氏名：" + tmpRequest.姓_漢字 + "　" + tmpRequest.名_漢字 + "\n\n";
                                            strMsg += "重複者申請番号：" + chkRequest[i].申請番号 + "\n";
                                            strMsg += "重複者ユーザID：" + chkRequest[i].ユーザID + "\n";
                                            strMsg += "重複者職員番号：" + chkRequest[i].職員番号 + "\n";
                                            strMsg += "重複者氏名：" + chkRequest[i].姓_漢字 + "　" + chkRequest[i].名_漢字 + "\n";
                                            MessageBox.Show(strMsg);
                                            return;
                                        }
                                    }
                                }
                            }

                            //事務職員誤登録チェック(事務職員表示がチェックされてるときはチェックしない)
                            if ((bool)chkDevFlg.IsChecked == false)
                            {
                                if (tmpRequest.事務フラグ == "TRUE" && tmpRequest.連絡先 == "kk-ajyoho2@jimu.gunma-u.ac.jp" && strAppUsr != "jimu")
                                {
                                    string strMsg = "下記の申請は事務登録用です。\n処理を中断します。\n\n";
                                    strMsg += "申請番号：" + tmpRequest.申請番号 + "\n";
                                    strMsg += "ユーザID：" + tmpRequest.ユーザID + "\n";
                                    strMsg += "職員番号：" + tmpRequest.職員番号 + "\n";
                                    strMsg += "氏名：" + tmpRequest.姓_漢字 + "　" + tmpRequest.名_漢字 + "\n\n";
                                    MessageBox.Show(strMsg);
                                    return;
                                }
                            }

                        }
                        else
                        {

                            // 再申請除外フラグが選択された場合
                            if ((bool)chkexceptReFlg.IsChecked && tmpRequest.再申請 == "TRUE")
                            {
                                bExistFlg = true;
                            }
                            else
                            {
                                // 退職者除外フラグが選択された場合
                                if ((bool)chkexceptExitFlg.IsChecked)
                                {
                                    // LdapAllUsrをチェックしてIDがあり，アカウントステータスが13(退職)だったら除外フラグを有効化
                                    chkSql = "select * from LdapAllUsr where ldap_id = '" + tmpRequest.ユーザID + "' and status_code = '13' ";

                                    iResult = dbAObj.SQLExecutionExistence(chkSql);
                                    if (iResult >= 1)
                                    {
                                        bExistFlg = true;
                                    }
                                }
                            }
                        }

                        #endregion<エラーチェック処理>

                        // LDAPにデータがないものだけCSV作成する。
                        // requestテーブル選択時は退職者チェック処理しか実行されないので登録済みのデータも出力される。
                        if (!bExistFlg)
                        {
                            // 基盤係が登録する際はすべて所属を事務局にする
                            // 事務職員登録の場合所属変換はしていないので独自に値を設定する。
                            string strldapfaculty = string.Empty;
                            string strldapdept = string.Empty;
                            string strldapoccupation = string.Empty;
                            string strVlan = "2560";
                            if (strAppUsr == "jimu")
                            {
                                strldapfaculty = "事務局";
                                strldapdept = "";
                                strldapoccupation = strjimuoccupation;
                            }
                            else
                            {
                                strldapfaculty = tmpConvert[0].全学所属;
                                strldapdept = tmpConvert[0].全学学科;
                                strldapoccupation = tmpConvert[0].全学職種;
                                // 20201006 vlanをldapに登録するため修正
                                if (tmpConvert[0].AA荒牧D != string.Empty && tmpConvert[0].AA昭和D == string.Empty && tmpConvert[0].AA桐生D == string.Empty)
                                {
                                    strVlan = tmpConvert[0].AA荒牧部.Substring(0, tmpConvert[0].AA荒牧部.IndexOf("."));
                                }
                                else if (tmpConvert[0].AA荒牧D == string.Empty && tmpConvert[0].AA昭和D != string.Empty && tmpConvert[0].AA桐生D == string.Empty)
                                {
                                    strVlan = tmpConvert[0].AA昭和部.Substring(0, tmpConvert[0].AA昭和部.IndexOf("."));
                                }
                                else if (tmpConvert[0].AA荒牧D == string.Empty && tmpConvert[0].AA昭和D == string.Empty && tmpConvert[0].AA桐生D != string.Empty)
                                {
                                    strVlan = tmpConvert[0].AA桐生部.Substring(0, tmpConvert[0].AA桐生部.IndexOf("."));
                                }
                                else
                                {
                                    if (int.Parse(tmpRequest.内線番号) >= 7000 && int.Parse(tmpRequest.内線番号) <= 7699)
                                    {
                                        strVlan = tmpConvert[0].AA荒牧部.Substring(0, tmpConvert[0].AA荒牧部.IndexOf("."));
                                    }
                                    else if ((int.Parse(tmpRequest.内線番号) >= 7700 && int.Parse(tmpRequest.内線番号) <= 8999) || (int.Parse(tmpRequest.内線番号) >= 4000 && int.Parse(tmpRequest.内線番号) <= 4999))
                                    {
                                        strVlan = tmpConvert[0].AA昭和部.Substring(0, tmpConvert[0].AA昭和部.IndexOf("."));
                                    }
                                    else if (int.Parse(tmpRequest.内線番号) >= 1000 && int.Parse(tmpRequest.内線番号) <= 1999)
                                    {
                                        strVlan = tmpConvert[0].AA桐生部.Substring(0, tmpConvert[0].AA桐生部.IndexOf("."));
                                    }
                                }
                            }


                            // 表示名が全角の場合は半角に変換する。
                            var strCnSei = Regex.Replace(tmpRequest.姓_漢字, "[ａ-ｚ]", p => ((char)(p.Value[0] - 'ａ' + 'a')).ToString());
                            var strCnMei = Regex.Replace(tmpRequest.名_漢字, "[ａ-ｚ]", p => ((char)(p.Value[0] - 'ａ' + 'a')).ToString());
                            string strSeiEn = Regex.Replace(strCnSei, "[Ａ-Ｚ]", p => ((char)(p.Value[0] - 'Ａ' + 'A')).ToString());
                            string strMeiEn = Regex.Replace(strCnMei, "[Ａ-Ｚ]", p => ((char)(p.Value[0] - 'Ａ' + 'A')).ToString());

                            // アカウントアダプターオンリーフラグが立っている場合はCSVデータは追加しない
                            if (!bAAonlyFlg)
                            {

                                lCSV.Add(new dtclsCSV
                                {
                                    ChangeType = "add",
                                    全学ID = tmpRequest.ユーザID,
                                    //姓_日 = tmpRequest.姓_漢字,
                                    //名_日 = tmpRequest.名_漢字,
                                    姓_日 = strSeiEn,
                                    名_日 = strMeiEn,
                                    姓_英 = tmpRequest.姓_英字,
                                    名_英 = tmpRequest.名_英字,
                                    パスワード = tmpRequest.パスワード,
                                    所属 = strldapfaculty,
                                    アカウントステータス = lConStat[0].Trim(),
                                    学科 = strldapdept,
                                    職員番号 = tmpRequest.職員番号,
                                    職名 = strldapoccupation,
                                    雇用形態 = strlmployment,
                                    連絡先メール = tmpRequest.連絡先,
                                    内線番号 = tmpRequest.内線番号,
                                    //電話番号 = tmpRequest.電話番号, 電話番号は学外からでも変更できてしまうのでLDAPには登録しないので空文字に変更
                                    電話番号 = "",
                                    eduPersonAffiliation = lConStat[1].Trim(),
                                    人事所属 = tmpRequest.係講座等,
                                    人事部局 = tmpRequest.所属,
                                    人事職種 = tmpRequest.職種,
                                    有線VLANID = strVlan
                                });

                            }

                            // 事務の場合はアカウントアダプターの登録は不要
                            if (strAppUsr != "jimu")
                            {
                                lAA.Add(new dtclsAA
                                {
                                    ユーザID = tmpRequest.ユーザID,
                                    ディレクトリ_荒牧 = tmpConvert[0].AA荒牧D,
                                    所属部署_荒牧 = tmpConvert[0].AA荒牧部,
                                    ディレクトリ_昭和 = tmpConvert[0].AA昭和D,
                                    所属部署_昭和 = tmpConvert[0].AA昭和部,
                                    ディレクトリ_桐生 = tmpConvert[0].AA桐生D,
                                    所属部署_桐生 = tmpConvert[0].AA桐生部
                                });
                            }

                        }

                        iChkCount = 0;
                    }
                }

                grdCSV.ItemsSource = lCSV;
                grdAA.ItemsSource = lAA;

                //サブネット管理者に連絡が必要な場合に送信メッセージをクリップボードに出力

                //対象者格納用リスト宣言
                List<string> lmol = new List<string>(); //分子化学対象者リスト
                List<string> llnt = new List<string>(); //知能機械創製対象者リスト
                List<string> lenv = new List<string>(); //環境創生対象者リスト
                List<string> lele = new List<string>(); //電子情報対象者リスト
                List<string> lsoc = new List<string>(); //社会情報学部対象者リスト

                //メッセージ格納用変数宣言
                string strSubnetMsg = string.Empty;

                // 共通文面宣言
                string strcommon1 = "総合情報メディアセンターです。\n";
                strcommon1 += "お世話になっております。\n\n";
                strcommon1 += "本日，下記のユーザの全学認証アカウントを作成いたしました。\n\n";

                string strcommon2 = "ご変更の場合には，部局サブネット管理者向け機器管理システム\n";
                strcommon2 += "https://drs2022.media.gunma-u.ac.jp/admin/login\n";
                strcommon2 += "よりログインいただき，サブネットを変更していただきますよう\n";
                strcommon2 += "お願いいたします。\n\n";
                strcommon2 += "以上です。\n";
                strcommon2 += "よろしくお願いいたします。";

                // VLANID保持
                string mol_id = string.Empty;
                string lnt_id = string.Empty;
                string env_id = string.Empty;
                string ele_id = string.Empty;
                string soc_id = string.Empty;

                for (int i=0;i<lCSV.Count;i++)
                {
                    switch (lCSV[i].学科)
                    {
                        case "分子科学部門":
                            lmol.Add("・" + lCSV[i].姓_日 + " " + lCSV[i].名_日 + "様(全学認証アカウント：" + lCSV[i].全学ID + ")");
                            mol_id = lCSV[i].有線VLANID;
                            break;
                        case "知能機械創製部門":
                            llnt.Add("・" + lCSV[i].姓_日 + " " + lCSV[i].名_日 + "様(全学認証アカウント：" + lCSV[i].全学ID + ")");
                            lnt_id = lCSV[i].有線VLANID;
                            break;
                        case "環境創生部門":
                            lenv.Add("・" + lCSV[i].姓_日 + " " + lCSV[i].名_日 + "様(全学認証アカウント：" + lCSV[i].全学ID + ")");
                            env_id = lCSV[i].有線VLANID;
                            break;
                        case "電子情報部門":
                            lele.Add("・" + lCSV[i].姓_日 + " " + lCSV[i].名_日 + "様(全学認証アカウント：" + lCSV[i].全学ID + ")");
                            ele_id = lCSV[i].有線VLANID;
                            break;
                        case "情報学科":
                            lsoc.Add("・" + lCSV[i].姓_日 + " " + lCSV[i].名_日 + "様(全学認証アカウント：" + lCSV[i].全学ID + ")");
                            soc_id = lCSV[i].有線VLANID;
                            break;
                    }
                }

                // 分子化学部門用文面作成
                if (lmol.Count > 0)
                {
                    strSubnetMsg = string.Empty;

                    strSubnetMsg = "分子科学部門\n\n\n\n";
                    strSubnetMsg += strcommon1;
                    for (int f = 0; f < lmol.Count; f++)
                    {
                        strSubnetMsg += lmol[f] + "\n";
                    }
                    strSubnetMsg += "\n当該ユーザのネットワークは現在，サブネット：" + mol_id + ".分子科学部門となっています。\n\n";
                    strSubnetMsg += strcommon2;

                    Clipboard.SetText(strSubnetMsg);
                    MessageBox.Show("分子化学部門のサブネット管理者に連絡が必要なアカウントが含まれています。\n※文面をクリップボードに出力しました。");
                }

                // 知能機械創製部門文明作成
                if (llnt.Count > 0)
                {
                    strSubnetMsg = string.Empty;

                    strSubnetMsg = "知能機械創製部門\n\n\n\n";
                    strSubnetMsg += strcommon1;
                    for (int f = 0; f < llnt.Count; f++)
                    {
                        strSubnetMsg += llnt[f] + "\n";
                    }
                    strSubnetMsg += "\n当該ユーザのネットワークは現在，サブネット：" + lnt_id + ".知能機械創製部門となっています。\n\n";
                    strSubnetMsg += strcommon2;

                    Clipboard.SetText(strSubnetMsg);
                    MessageBox.Show("知能機械創製部門のサブネット管理者に連絡が必要なアカウントが含まれています。\n※文面をクリップボードに出力しました。");
                }

                // 環境創生部門用文面作成
                if (lenv.Count > 0)
                {
                    strSubnetMsg = string.Empty;

                    strSubnetMsg = "環境創生部門\n\n\n\n";
                    strSubnetMsg += strcommon1;
                    for (int f = 0; f < lenv.Count; f++)
                    {
                        strSubnetMsg += lenv[f] + "\n";
                    }
                    strSubnetMsg += "\n当該ユーザのネットワークは現在，サブネット：" + env_id + ".環境創生部門となっています。\n\n";
                    strSubnetMsg += strcommon2;

                    Clipboard.SetText(strSubnetMsg);
                    MessageBox.Show("環境創生部門のサブネット管理者に連絡が必要なアカウントが含まれています。\n※文面をクリップボードに出力しました。");
                }

                // 電子情報部門用文面作成
                if (lele.Count > 0)
                {
                    strSubnetMsg = string.Empty;

                    strSubnetMsg = "電子情報部門\n\n\n\n";
                    strSubnetMsg += strcommon1;
                    for (int f = 0; f < lele.Count; f++)
                    {
                        strSubnetMsg += lele[f] + "\n";
                    }
                    strSubnetMsg += "\n当該ユーザのネットワークは現在，サブネット：" + ele_id + ".電子情報部門となっています。\n\n";
                    strSubnetMsg += strcommon2;

                    Clipboard.SetText(strSubnetMsg);
                    MessageBox.Show("電子情報部門のサブネット管理者に連絡が必要なアカウントが含まれています。\n※文面をクリップボードに出力しました。");
                }

                // 社会情報学部用文面作成
                if (lsoc.Count > 0)
                {
                    strSubnetMsg = string.Empty;

                    strSubnetMsg = "社会情報学部\n\n\n\n";
                    strSubnetMsg += strcommon1;
                    for (int f = 0; f < lsoc.Count; f++)
                    {
                        strSubnetMsg += lsoc[f] + "\n";
                    }
                    strSubnetMsg += "\n当該ユーザのネットワークは現在，サブネット：" + soc_id + ".社会情報学部となっています。\n\n";
                    strSubnetMsg += strcommon2;

                    Clipboard.SetText(strSubnetMsg);
                    MessageBox.Show("社会情報学部のサブネット管理者に連絡が必要なアカウントが含まれています。\n※文面をクリップボードに出力しました。");
                }

                //サブネットが2560なのに職種が事務職員じゃない場合に変換中断
                for (int i = 0; i < lAA.Count; i++)
                {
                    if (lAA[i].所属部署_荒牧 == "2560.ゲスト" || lAA[i].所属部署_昭和 == "2560.ゲスト" || lAA[i].所属部署_桐生 == "2560.ゲスト")
                    {
                        if(lCSV[i].職名 != "事務職員" && lCSV[i].職名 != "事務補佐員" && lCSV[i].職名 != "技術職員" && lCSV[i].職名 != "その他" && lCSV[i].職名 != "技術補佐員" && lCSV[i].職名 != "医療系職員")
                        {
                            MessageBox.Show("事務職員で職名が誤っているデータがあります。\n変換テーブルを修正してください。");
                            grdCSV.ItemsSource = null;
                            grdAA.ItemsSource = null;
                            break;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message + "\n\nErrdetail：" + ex.StackTrace;
                MessageBox.Show(strErrMsg);
            }
        }
        #endregion

        #region<参照ボタンクリックイベント>
        private void btnFolderSerch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //パス取得用ダイアログ表示
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "CSV保存先選択";
                sfd.FilterIndex = 1;
                if (strAppUsr == "jimu")
                {
                    sfd.FileName = "jimustaff.csv";
                }
                else
                {
                    sfd.FileName = "staff.csv";
                }
                sfd.Filter = "csvファイル(.csv)|*.csv";
                bool bResult = (bool)sfd.ShowDialog();

                if (bResult)
                {
                    if(sfd.SafeFileName != "staff.csv" && strAppUsr == "media")
                    {
                        MessageBox.Show("ファイル名が変更されました。\n処理を中断します。");

                        return;
                    }
                    else if (sfd.SafeFileName != "jimustaff.csv" && strAppUsr == "jimu")
                    {
                        MessageBox.Show("ファイル名が変更されました。\n処理を中断します。");

                        return;
                    }
                    else
                    {
                        txtFolder.Text = sfd.FileName;
                    }
                }

            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
            }
        }
        #endregion

        #region<CSV出力ボタンクリックイベント>
        private void btnCsvMake_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // モードによって置換用文字列を変更
                string strfilename = string.Empty;

                if(strAppUsr == "media")
                {
                    strfilename = "\\staff.csv";
                }
                else if (strAppUsr == "jimu")
                {
                    strfilename = "\\jimustaff.csv";
                }

                //登録用CSV出力
                if (Directory.Exists(txtFolder.Text.Trim().Replace(strfilename, "")))
                {
                    StreamWriter writer = new StreamWriter(txtFolder.Text.Trim(),false,Encoding.GetEncoding("Shift-JIS"));
                    writer.WriteLine("ChangeType,全学ID,姓(日),名(日),姓(英),名(英),パスワード,所属,アカウントステータス,学科,職員番号,職名,雇用形態,連絡先メール,内線番号,tellephone,eduPersonAffiliation,人事-所属,人事-部局,人事-職種,有線VLANID");

                    foreach (dtclsCSV tmpCSV in lCSV)
                    {
                        string strline = tmpCSV.ChangeType + ",";
                        strline += tmpCSV.全学ID + ",";
                        strline += tmpCSV.姓_日 + ",";
                        strline += tmpCSV.名_日 + ",";
                        strline += tmpCSV.姓_英 + ",";
                        strline += tmpCSV.名_英 + ",";
                        strline += tmpCSV.パスワード + ",";
                        strline += tmpCSV.所属 + ",";
                        strline += tmpCSV.アカウントステータス + ",";
                        strline += tmpCSV.学科 + ",";
                        strline += tmpCSV.職員番号 + ",";
                        strline += tmpCSV.職名 + ",";
                        strline += tmpCSV.雇用形態 + ",";
                        strline += tmpCSV.連絡先メール + ",";
                        strline += tmpCSV.内線番号 + ",";
                        strline += tmpCSV.電話番号 + ",";
                        strline += tmpCSV.eduPersonAffiliation + ",";
                        strline += tmpCSV.人事所属 + ",";
                        strline += tmpCSV.人事部局 + ",";
                        strline += tmpCSV.人事職種 + ",";
                        strline += tmpCSV.有線VLANID;
                        writer.WriteLine(strline);
                    }

                    writer.Close();
                }
                else
                {
                    string strErrMsg = "指定されたフォルダが見つかりません。\n処理を中断します。";
                    MessageBox.Show(strErrMsg);
                    return;
                }

                string strMsg = "CSVファイルが出力されました。";

                // requestテーブルが選択されているときはチェック処理が実施されないので
                // そのまま登録しないよう注意メッセージを表示
                if((string)cmbTableList.SelectedValue == "request")
                {
                    strMsg += "\nそのまま登録するとエラーが発生するので加工して実行してください。";
                }

                //復帰ユーザーメール送付バッチ出力
                if (lBat.Count >= 1)
                {
                    StreamWriter writerBat = new StreamWriter(txtFolder.Text.Trim().Replace("\\staff.csv", "\\SendMail.bat"), false, Encoding.GetEncoding("Shift-JIS"));

                    foreach (string tmpBat in lBat)
                    {
                        writerBat.WriteLine(tmpBat);
                    }

                    writerBat.Close();

                    strMsg += "\nメール送付バッチが出力されました。\nサーバ上で実行してアカウント復帰者に通知を送付してください。";
                }

                strMsg += "\n出力先のフォルダを表示します。";

                MessageBox.Show(strMsg);

                Process.Start(txtFolder.Text.Trim().Replace(strfilename, ""));

            }
            catch (Exception ex)
            {
                string strErrMsg = "想定外のエラーが発生したため処理を中断します。\n ErrMsg：" + ex.Message;
                MessageBox.Show(strErrMsg);
            }
        }
        #endregion

        #region<未申請・全学一覧参照ボタンクリックイベント>
        private void btnReference_Click(object sender, RoutedEventArgs e)
        {
            //未申請・全学一覧参照ダイアログをインスタンス化
            Reference winRef = new Reference(dbAObj);

            winRef.ShowDialog();
        }
        #endregion

        #endregion

        #region<メソッド>

        #region<DBデータ取得メソッド>
        private void getDBData()
		{
            //コンボボックスからテーブル名を取得
            string strCmbValue = (string)cmbTableList.SelectedValue;

            //申請テーブルのデータを取得
            if (strCmbValue == "request" || strCmbValue == "UnIssuedRecode")
            {
                lRequest = dbAObj.getRequest(txtUsr.Text,txtNo.Text,txtSei.Text,txtMei.Text,txtFaculty.Text,txtNaisen.Text,strCmbValue, (bool)chkDevFlg.IsChecked);

                //取得したデータを画面に表示
                setRequest(lRequest.Count-1);
            }
            //人事データを取得
            else if(strCmbValue.Contains("id_number"))
            {
                grdDBdata.ItemsSource = dbAObj.getIdNumber(strCmbValue);
            }
            else
            {
                MessageBox.Show("想定外のテーブルのためデータを取得できませんでした。");
            }
		}
        #endregion

        #region<requestデータセットメソッド>
        /// <summary>
        /// 申請リストにDBから取得したデータを表示する
        /// </summary>
        /// <param name="iSelectRow">設定された行数にフォーカスをセット</param>
        private void setRequest(int iSelectRow)
        {
            grdDBdata.ItemsSource = null;
            grdDBdata.ItemsSource = lRequest;

            //選択行にフォーカス
            if (iSelectRow >= 0)
            {
                grdDBdata.Focus();
                grdDBdata.SelectedIndex = iSelectRow;
                grdDBdata.CurrentCell = new DataGridCellInfo(grdDBdata.Items[iSelectRow], grdDBdata.Columns[0]);
            }
        }
        #endregion


        #region<処理対象チェックメソッド>
        /// <summary>
        /// 処理対象チェック処理　行クリック時と一括選択時に使用
        /// </summary>
        /// <param name="iStart"></param>
        /// <param name="iEnd"></param>
        private void SetSelect(int iStart,int iEnd)
        {
            int iSetRow = 0;

            if(iStart == iEnd)
            {
                iSetRow = iStart;

                if (lRequest[iStart].処理対象)
                {
                    lRequest[iStart].setBool(false);
                }
                else
                {
                    lRequest[iStart].setBool(true);
                }
            }
            else
            {
                for(int i=iStart;i<=iEnd;i++)
                {
                    //後ろから検索して時短
                    for(int f=lRequest.Count-1;f>=0;f--)
                    {
                        if (lRequest[f].申請番号 == i.ToString())
                        {
                            lRequest[f].setBool(true);

                            iSetRow = f;

                            break;
                        }
                    }
                }
            }

            setRequest(iSetRow);
        }
        #endregion

        #region<変換マスタ修正画面オープンメソッド>
        private void OpenMst(List<string> strErrRecode)
        {
            //変換テーブル管理ダイアログをインスタンス化
            //ConvertMst winMst = new ConvertMst(dbAObj, strAccessDBPath,strErrRecode);
            ConvertMst winMst = new ConvertMst(dbAObj, strErrRecode);

            winMst.ShowDialog();
        }
        #endregion

        #endregion
    }
}
