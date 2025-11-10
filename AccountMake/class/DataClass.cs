namespace AccountMake
{
    #region<dtclsIdNumber>
    public class dtclsIdNumber
    {
        public string 職員番号 { get; set; }
        public string 氏名 { get; set; }
        public string 職種 { get; set; }
        public string 所属 { get; set; }
        public string 係講座等 { get; set; }
        public string 生年月日 { get; set; }
    }
    #endregion

    #region<dtclsRequest>
    public class dtclsRequest
    {
        public bool 処理対象 { get; set; }
        public string 申請番号 { get; set; }
        public string 姓_漢字 { get; set; }
        public string 名_漢字 { get; set; }
        public string 姓_英字 { get; set; }
        public string 名_英字 { get; set; }
        public string 職種 { get; set; }
        public string 所属 { get; set; }
        public string 係講座等 { get; set; }
        public string 職員番号 { get; set; }
        public string 内線番号 { get; set; }
        public string 電話番号 { get; set; }
        public string 連絡先 { get; set; }
        public string ユーザID { get; set; }
        public string パスワード { get; set; }
        public string 事務フラグ { get; set; }
        public string 再申請 { get; set; }
        public string 入力日時 { get; set; }

        public void setBool(bool _処理対象)
        {
            処理対象 = _処理対象;
        }
    }
    #endregion

    #region<dtclsConvert>
    public class dtclsConvert
    {
        public int ID { get; set; }
        public string 人事所属 { get; set; }
        public string 人事係講座 { get; set; }
        public string 人事職種 { get; set; }
        public string 全学所属 { get; set; }
        public string 全学学科 { get; set; }
        public string 全学職種 { get; set; }
        public string AA荒牧D { get; set; }
        public string AA荒牧部 { get; set; }
        public string AA昭和D { get; set; }
        public string AA昭和部 { get; set; }
        public string AA桐生D { get; set; }
        public string AA桐生部 { get; set; }
    }
    #endregion

    #region<dtclsCSV>
    public class dtclsCSV
    {
        //新LDAP対応 20180316
        //public string ChangeType { get; set; }
        //public string 全学ID { get; set; }
        //public string 姓_日 { get; set; }
        //public string 名_日 { get; set; }
        //public string 姓_英 { get; set; }
        //public string 名_英 { get; set; }
        //public string パスワード { get; set; }
        //public string 所属 { get; set; }
        //public string 学科 { get; set; }
        //public string 職員番号 { get; set; }
        //public string 職名 { get; set; }
        //public string 雇用形態 { get; set; }
        //public string 連絡先メール { get; set; }
        //public string 内線番号 { get; set; }

        public string ChangeType { get; set; }
        public string 全学ID { get; set; }
        public string 姓_日 { get; set; }
        public string 名_日 { get; set; }
        public string 姓_英 { get; set; }
        public string 名_英 { get; set; }
        public string パスワード { get; set; }
        public string 所属 { get; set; }
        public string アカウントステータス { get; set; }
        public string 学科 { get; set; }
        public string 職員番号 { get; set; }
        public string 職名 { get; set; }
        public string 雇用形態 { get; set; }
        public string 連絡先メール { get; set; }
        public string 内線番号 { get; set; }
        public string 電話番号 { get; set; }
        public string eduPersonAffiliation { get; set; }
        public string 人事所属 { get; set; }
        public string 人事部局 { get; set; }
        public string 人事職種 { get; set; }
        public string 有線VLANID { get; set; }
    }
    #endregion

    #region<dtclsAA>
    public class dtclsAA
    {
        public string ユーザID { get; set; }
        public string ディレクトリ_荒牧 { get; set; }
        public string 所属部署_荒牧 { get; set; }
        public string ディレクトリ_昭和 { get; set; }
        public string 所属部署_昭和 { get; set; }
        public string ディレクトリ_桐生 { get; set; }
        public string 所属部署_桐生 { get; set; }
    }
    #endregion

    #region<dtclsLdap>
    public class dtclsLdap
    {
        public string ユーザID { get; set; }
        public string 氏名 { get; set; }
        public string 職員番号 { get; set; }
        public string アカウントステータス { get; set; }
        public string ステータスコード { get; set; }
        public string 全学Gmail { get; set; }
        public string 全学所属 { get; set; }
        public string 全学学科 { get; set; }
        public string 全学職種 { get; set; }
        public string 事務所属 { get; set; }
        public string 事務係講座 { get; set; }
        public string 事務職種 { get; set; }
        public string 事務フラグ { get; set; }
        public string DB登録日時 { get; set; }
    }
    #endregion

    #region<dtclsUnIssued>
    public class dtclsUnIssued
    {
        public string 職員番号 { get; set; }
        public string 氏名 { get; set; }
        public string 事務所属 { get; set; }
        public string 事務係講座 { get; set; }
        public string 事務職種 { get; set; }
        public string DB更新日時 { get; set; }
        public string DB登録日時 { get; set; }
    }
    #endregion
}
