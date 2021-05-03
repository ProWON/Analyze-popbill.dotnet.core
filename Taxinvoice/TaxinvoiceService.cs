using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;


namespace Popbill.Taxinvoice
{
    public enum MgtKeyType
    {
        SELL,
        BUY,
        TRUSTEE
    }
    //TaxinvoiceService : BaseService ": BaseService를 사용하여 BaseService 상속"
    public class TaxinvoiceService : BaseService
    {
        //base() 부모클래스 생성자를 호출 TaxinvoiceService로 들어온 파라미터 명을 base에 사용 가능
        //자식 인스턴스 생성될때 부모인스턴스 생성 하지만 자식따로 부모따로로 생각하면 안됨
        //Taxinvoice 인스턴스 생성전 부모 인스턴스 생성됨 둘은 하나임 이렇게 생각하는게 좋을듯
        public TaxinvoiceService(string LinkID, string SecretKey) : base(LinkID, SecretKey)
        {
            this.AddScope("110");
        }


        public TaxinvoiceService(string LinkID, string SecretKey, bool ProxyYN, string ProxyAddress, string ProxyUserName, string ProxyPassword)
            : base(LinkID, SecretKey, ProxyYN, ProxyAddress, ProxyUserName, ProxyPassword)
        {
            this.AddScope("110");
        }
        #region Issue API

        //문서번호 확인
        public bool CheckMgtKeyInUse(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            try
            {
                TaxinvoiceInfo response =
                    httpget<TaxinvoiceInfo>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, UserID);

                return string.IsNullOrEmpty(response.itemKey) == false;
            }
            catch (PopbillException pe)
            {
                if (pe.code == -11000005) return false;

                throw pe;
            }
        }
        //즉시 발행 - JSON
        public IssueResponse RegistIssue(string CorpNum, string taxinvoice, string UserID = null)
        {
            if (string.IsNullOrEmpty(taxinvoice)) throw new PopbillException(-99999999, "세금계산서 정보가 입력되지 않았습니다.");

            return httppost<IssueResponse>("/Taxinvoice", CorpNum, taxinvoice, "ISSUE", null, UserID);
        }

        //즉시 발행
        public IssueResponse RegistIssue(string CorpNum, Taxinvoice taxinvoice, bool WriteSpecification = false,
            bool ForceIssue = false, string DealinvoiceMgtKey = null, string Memo = null, string EmailSubject = null,
            string UserID = null)
        {
            if (taxinvoice == null) throw new PopbillException(-99999999, "세금계산서 정보가 입력되지 않았습니다.");

            taxinvoice.writeSpecification = WriteSpecification;
            taxinvoice.forceIssue = ForceIssue;
            taxinvoice.dealInvoiceMgtKey = DealinvoiceMgtKey;
            taxinvoice.memo = Memo;
            taxinvoice.emailSubject = EmailSubject;

            string PostData = toJsonString(taxinvoice);

            return httppost<IssueResponse>("/Taxinvoice", CorpNum, PostData, "ISSUE", null, UserID);
        }

        //임시저장 - JSON
        public Response Register(string CorpNum, string taxinvoice, string UserID = null)
        {
            if (string.IsNullOrEmpty(taxinvoice)) throw new PopbillException(-99999999, "세금계산서 정보가 입력되지 않았습니다.");

            return httppost<Response>("/Taxinvoice", CorpNum, taxinvoice, null, null, UserID);
        }


        //임시저장
        public Response Register(string CorpNum, Taxinvoice taxinvoice, bool WriteSpecification = false,
            string DealinvoiceMgtKey = null, string UserID = null)
        {
            if (taxinvoice == null) throw new PopbillException(-99999999, "세금계산서 정보가 입력되지 않았습니다.");

            string PostData = toJsonString(taxinvoice);

            if (WriteSpecification)
            {
                PostData = "{\"writeSpecification\":true," + PostData.Substring(1);
            }

            return httppost<Response>("/Taxinvoice", CorpNum, PostData, null, null, UserID);
        }

        //수정 - String JSON
        public Response Update(string CorpNum, MgtKeyType KeyType, string MgtKey, string taxinvoice,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");
            if (string.IsNullOrEmpty(taxinvoice)) throw new PopbillException(-99999999, "세금계산서 정보가 입력되지 않았습니다.");

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, taxinvoice, "PATCH",
                null, UserID);
        }

        //수정
        public Response Update(string CorpNum, MgtKeyType KeyType, string MgtKey, Taxinvoice taxinvoice,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");
            if (taxinvoice == null) throw new PopbillException(-99999999, "세금계산서 정보가 입력되지 않았습니다.");

            string PostData = toJsonString(taxinvoice);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData, "PATCH",
                null, UserID);
        }

        //발행
        public IssueResponse Issue(string CorpNum, MgtKeyType KeyType, string MgtKey, bool ForceIssue = false,
            string Memo = null, string EmailSubject = null, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            IssueRequest request = new IssueRequest {memo = Memo, emailSubject = EmailSubject, forceIssue = ForceIssue};

            string PostData = toJsonString(request);

            return httppost<IssueResponse>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData, "ISSUE",
                null, UserID);
        }

        //발행취소
        public Response CancelIssue(string CorpNum, MgtKeyType KeyType, string MgtKey, string Memo = null,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            MemoRequest request = new MemoRequest {memo = Memo};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData,
                "CANCELISSUE", null, UserID);
        }

        //발행예정
        public Response Send(string CorpNum, MgtKeyType KeyType, string MgtKey, string Memo = null,
            string EmailSubject = null, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            MemoRequest request = new MemoRequest {memo = Memo, emailSubject = EmailSubject};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData, "SEND",
                null, UserID);
        }

        //발행예정 취소
        public Response CancelSend(string CorpNum, MgtKeyType KeyType, string MgtKey, string Memo = null,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            MemoRequest request = new MemoRequest {memo = Memo};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData,
                "CANCELSEND", null, UserID);
        }

        //발행예정 승인
        public Response Accept(string CorpNum, MgtKeyType KeyType, string MgtKey, string Memo = null,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            MemoRequest request = new MemoRequest {memo = Memo};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData, "ACCEPT",
                null, UserID);
        }

        //발행예정 거부
        public Response Deny(string CorpNum, MgtKeyType KeyType, string MgtKey, string Memo = null,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            MemoRequest request = new MemoRequest {memo = Memo};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData, "DENY",
                null, UserID);
        }

        //삭제
        public Response Delete(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, null, "DELETE", null,
                UserID);
        }

        //즉시 요청 - JSON
        public Response RegistRequest(string CorpNum, string taxinvoice, string UserID = null)
        {
            if (string.IsNullOrEmpty(taxinvoice)) throw new PopbillException(-99999999, "세금계산서 정보가 입력되지 않았습니다.");

            return httppost<Response>("/Taxinvoice", CorpNum, taxinvoice, "REQUEST", null, UserID);
        }

        //즉시 요청
        public Response RegistRequest(string CorpNum, Taxinvoice taxinvoice, string Memo = null, string UserID = null)
        {
            if (taxinvoice == null) throw new PopbillException(-99999999, "세금계산서 정보가 입력되지 않았습니다.");

            taxinvoice.memo = Memo;

            string PostData = toJsonString(taxinvoice);

            return httppost<Response>("/Taxinvoice", CorpNum, PostData, "REQUEST", null, UserID);
        }

        //역발행요청
        public Response Request(string CorpNum, MgtKeyType KeyType, string MgtKey, string Memo = null,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            MemoRequest request = new MemoRequest {memo = Memo};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData, "REQUEST",
                null, UserID);
        }

        //역발행요청 취소
        public Response CancelRequest(string CorpNum, MgtKeyType KeyType, string MgtKey, string Memo = null,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            MemoRequest request = new MemoRequest {memo = Memo};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData,
                "CANCELREQUEST", null, UserID);
        }

        //역발행요청 거부
        public Response Refuse(string CorpNum, MgtKeyType KeyType, string MgtKey, string Memo = null,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            MemoRequest request = new MemoRequest {memo = Memo};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData, "REFUSE",
                null, UserID);
        }

        //국세청 즉시 전송
        public Response SendToNTS(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, null, "NTS", null,
                UserID);
        }

        #endregion

        #region Info API

        //상태 확인
        public TaxinvoiceInfo GetInfo(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            return httpget<TaxinvoiceInfo>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, UserID);
        }

        //상태 대량 확인
        public List<TaxinvoiceInfo> GetInfos(string CorpNum, MgtKeyType KeyType, List<string> MgtKeyList,
            string UserID = null)
        {
            if (MgtKeyList == null || MgtKeyList.Count == 0)
                throw new PopbillException(-99999999, "문서번호 목록이 입력되지 않았습니다.");

            string PostData = toJsonString(MgtKeyList);

            return httppost<List<TaxinvoiceInfo>>("/Taxinvoice/" + KeyType.ToString(), CorpNum, PostData, null, null,
                UserID);
        }

        //상세정보 확인
        public Taxinvoice GetDetailInfo(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            return httpget<Taxinvoice>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "?Detail", CorpNum, UserID);
        }

        //목록 조회
        public TISearchResult Search(string CorpNum, MgtKeyType KeyType, string DType, string SDate, string EDate,
            string[] State = null, string[] Type = null, string[] TaxType = null, string[] IssueType = null,
            bool? LateOnly = null, string TaxRegIDYN = null, string TaxRegIDType = null, string TaxRegID = null,
            int? Page = null, int? PerPage = null, string Order = null, string QString = null, string InterOPYN = null,
            string UserID = null, string[] RegType = null, string[] CloseDownState = null, string MgtKey = null)
        {
            if (string.IsNullOrEmpty(DType)) throw new PopbillException(-99999999, "검색일자 유형이 입력되지 않았습니다.");
            if (string.IsNullOrEmpty(SDate)) throw new PopbillException(-99999999, "시작일자가 입력되지 않았습니다.");
            if (string.IsNullOrEmpty(EDate)) throw new PopbillException(-99999999, "종료일자가 입력되지 않았습니다.");

            string uri = "/Taxinvoice/" + KeyType;
            uri += "?DType=" + DType;
            uri += "&SDate=" + SDate;
            uri += "&EDate=" + EDate;

            if (State != null) uri += "&State=" + string.Join(",", State);
            if (Type != null) uri += "&Type=" + string.Join(",", Type);
            if (TaxType != null) uri += "&TaxType=" + string.Join(",", TaxType);
            if (IssueType != null) uri += "&IssueType=" + string.Join(",", IssueType);
            if (RegType != null) uri += "&RegType=" + string.Join(",", RegType);
            if (CloseDownState != null) uri += "&CloseDownState=" + string.Join(",", CloseDownState);

            if (LateOnly != null)
            {
                if ((bool) LateOnly)
                {
                    uri += "&LateOnly=1";
                }
                else
                {
                    uri += "&LateOnly=0";
                }
            }

            if (TaxRegIDYN != null) uri += "&TaxRegIDYN=" + TaxRegIDYN;
            if (TaxRegIDType != null) uri += "&TaxRegIDType=" + TaxRegIDType;
            if (TaxRegID != null) uri += "&TaxRegID=" + TaxRegID;
            if (Page != null) uri += "&Page=" + Page.ToString();
            if (PerPage != null) uri += "&PerPage=" + PerPage.ToString();
            if (Order != null) uri += "&Order=" + Order;
            if (QString != null) uri += "&QString=" + QString;
            if (MgtKey != null) uri += "&MgtKey=" + MgtKey;
            if (InterOPYN != null) uri += "&InterOPYN=" + InterOPYN;

            return httpget<TISearchResult>(uri, CorpNum, UserID);
        }

        //상태 변경이력 확인
        public List<TaxinvoiceLog> GetLogs(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            return httpget<List<TaxinvoiceLog>>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "/Logs", CorpNum,
                UserID);
        }

        //세금계산서 문서함 관련 URL
        public string GetURL(string CorpNum, string TOGO, string UserID)
        {
            URLResponse response = httpget<URLResponse>("/Taxinvoice?TG=" + TOGO, CorpNum, UserID);

            return response.url;
        }

        #endregion

        #region PopUp/Print API

        //세금계산서 보기 URL
        public string GetPopUpURL(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            URLResponse response =
                httpget<URLResponse>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "?TG=POPUP", CorpNum, UserID);

            return response.url;
        }

        //세금계산서 보기 URL
        public string GetViewURL(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            URLResponse response =
                httpget<URLResponse>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "?TG=VIEW", CorpNum, UserID);

            return response.url;
        }

        public string GetPDFURL(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            URLResponse response =
                httpget<URLResponse>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "?TG=PDF", CorpNum, UserID);

            return response.url;
        }

        //tls) 세금계산서 인쇄 URL (공급자/공급받는자용 인쇄 팝업 뷰)
        public string GetPrintURL(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            URLResponse response =
                httpget<URLResponse>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "?TG=PRINT", CorpNum, UserID);

            return response.url;
        }

        // 구) 세금계산서 인쇄 URL 
        public string GetOldPrintURL(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            URLResponse response =
                httpget<URLResponse>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "?TG=PRINTOLD", CorpNum,
                    UserID);

            return response.url;
        }

        //세금계산서 인쇄 URL (공급받는자용 인쇄 팝업 뷰)
        public string GetEPrintURL(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            URLResponse response =
                httpget<URLResponse>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "?TG=EPRINT", CorpNum,
                    UserID);

            return response.url;
        }

       


        //세금계산서 대량 인쇄 URL
        public string GetMassPrintURL(string CorpNum, MgtKeyType KeyType, List<string> MgtKeyList, string UserID)
        {
            if (MgtKeyList == null || MgtKeyList.Count == 0)
                throw new PopbillException(-99999999, "문서번호 목록이 입력되지 않았습니다.");

            string PostData = toJsonString(MgtKeyList);

            URLResponse response = httppost<URLResponse>("/Taxinvoice/" + KeyType.ToString() + "?Print", CorpNum,
                PostData, null, null, UserID);

            return response.url;
        }

        //세금계산서 메일링크 URL
        public string GetMailURL(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            URLResponse response = httpget<URLResponse>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "?TG=MAIL",
                CorpNum, UserID);

            return response.url;
        }

        #endregion

        #region Add Ons API

        //팝빌 인감 및 첨부문서 등록 URL
        public string GetSealURL(string CorpNum, string UserID)
        {
            URLResponse response = httpget<URLResponse>("/?TG=SEAL", CorpNum, UserID);

            return response.url;
        }

        //첨부파일 추가
        public Response AttachFile(string CorpNum, MgtKeyType KeyType, string MgtKey, string FilePath,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(value: MgtKey))
                throw new PopbillException(code: -99999999, Message: "문서번호가 입력되지 않았습니다.");
            if (string.IsNullOrEmpty(value: FilePath))
                throw new PopbillException(code: -99999999, Message: "파일경로가 입력되지 않았습니다.");

            List<UploadFile> files = new List<UploadFile>();

            UploadFile file = new UploadFile
            {
                FieldName = "Filedata",
                FileName = System.IO.Path.GetFileName(path: FilePath),
                FileData = new FileStream(path: FilePath, mode: FileMode.Open, access: FileAccess.Read)
            };


            files.Add(item: file);

            return httppostFile<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "/Files", CorpNum, null,
                files, null, UserID);
        }

        //첨부파일 삭제
        public Response DeleteFile(string CorpNum, MgtKeyType KeyType, string MgtKey, string FileID,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");
            if (string.IsNullOrEmpty(FileID)) throw new PopbillException(-99999999, "파일 아이디가 입력되지 않았습니다.");

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "/Files/" + FileID, CorpNum,
                null, "DELETE", null, UserID);
        }

        //첨부파일 목록 확인
        public List<AttachedFile> GetFiles(string CorpNum, MgtKeyType KeyType, string MgtKey, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            return httpget<List<AttachedFile>>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey + "/Files", CorpNum,
                UserID);
        }

        //메일 전송
        public Response SendEmail(string CorpNum, MgtKeyType KeyType, string MgtKey, string Receiver,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            ResendRequest request = new ResendRequest {receiver = Receiver};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData, "EMAIL",
                null, UserID);
        }

        //문자 전송
        public Response SendSMS(string CorpNum, MgtKeyType KeyType, string MgtKey, string Sender, string Receiver,
            string Contents, string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            ResendRequest request = new ResendRequest {sender = Sender, receiver = Receiver, contents = Contents};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData,
                "SMS", null, UserID);
        }

        //팩스 전송
        public Response SendFAX(string CorpNum, MgtKeyType KeyType, string MgtKey, string Sender, string Receiver,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "문서번호가 입력되지 않았습니다.");

            ResendRequest request = new ResendRequest {sender = Sender, receiver = Receiver};

            string PostData = toJsonString(request);

            return httppost<Response>("/Taxinvoice/" + KeyType.ToString() + "/" + MgtKey, CorpNum, PostData,
                "FAX", null, UserID);
        }

        //전자명세서 첨부
        public Response AttachStatement(string CorpNum, MgtKeyType KeyType, string MgtKey, int DocItemCode,
            string DocMgtKey, string UserID = null)
        {
            string uri = "/Taxinvoice/" + KeyType + "/" + MgtKey + "/AttachStmt";

            DocRequest request = new DocRequest {ItemCode = DocItemCode, MgtKey = DocMgtKey};

            string PostData = toJsonString(request);

            return httppost<Response>(uri, CorpNum, PostData, null, null, UserID);
        }

        //전자명세서 첨부해제
        public Response DetachStatement(string CorpNum, MgtKeyType KeyType, string MgtKey, int DocItemCode,
            string DocMgtKey, string UserID = null)
        {
            string uri = "/Taxinvoice/" + KeyType + "/" + MgtKey + "/DetachStmt";

            DocRequest request = new DocRequest {ItemCode = DocItemCode, MgtKey = DocMgtKey};

            string PostData = toJsonString(request);

            return httppost<Response>(uri, CorpNum, PostData, null, null, UserID);
        }

        //유통사업자 메일 목록 확인
        public List<EmailPublicKey> GetEmailPublicKeys(string CorpNum, string UserID = null)
        {
            return httpget<List<EmailPublicKey>>("/Taxinvoice/EmailPublicKeys", CorpNum, UserID);
        }

        //문서번호 할당
        public Response AssignMgtKey(string CorpNum, MgtKeyType KeyType, string ItemKey, string MgtKey,
            string UserID = null)
        {
            if (string.IsNullOrEmpty(MgtKey)) throw new PopbillException(-99999999, "할당할 문서번호가 입력되지 않았습니다.");

            string PostData = "MgtKey=" + MgtKey;

            return httppost<Response>("/Taxinvoice/" + ItemKey + "/" + KeyType, CorpNum, PostData, null,
                "application/x-www-form-urlencoded; charset=utf-8", UserID);
        }

        //알림메일 전송목록 조회
        public List<EmailConfig> ListEmailConfig(string CorpNum, string UserID = null)
        {
            return httpget<List<EmailConfig>>("/Taxinvoice/EmailSendConfig", CorpNum, UserID);
        }

        //알림메일 전송설정 수정
        public Response UpdateEmailConfig(string CorpNum, string EmailType, bool SendYN, string UserID = null)
        {
            if (string.IsNullOrEmpty(EmailType)) throw new PopbillException(-99999999, "메일전송 타입이 입력되지 않았습니다.");

            string uri = "/Taxinvoice/EmailSendConfig?EmailType=" + EmailType + "&SendYN=" + SendYN;

            return httppost<Response>(uri, CorpNum, null, null, null, UserID);
        }

        #endregion

        #region Certificate API

        //공인인증서 등록 URL
        public string GetTaxCertURL(string CorpNum, string UserID)
        {
            URLResponse response = httpget<URLResponse>("/?TG=CERT", CorpNum, UserID);

            return response.url;
        }

        //공인인증서 만료일 확인
        public DateTime GetCertificateExpireDate(string CorpNum, string UserID = null)
        {
            CertResponse response = httpget<CertResponse>("/Taxinvoice?cfg=CERT", CorpNum, UserID);

            return DateTime.ParseExact(response.certificateExpiration, "yyyyMMddHHmmss", null);
        }

        //공인인증서 유효성 확인
        public Response CheckCertValidation(string CorpNum, string UserID = null)
        {
            return httpget<Response>("/Taxinvoice/CertCheck", CorpNum, UserID);
        }

        #endregion

        #region Point API

        //발행단가 확인
        public Single GetUnitCost(string CorpNum, string UserID = null)
        {
            UnitCostResponse response = httpget<UnitCostResponse>("/Taxinvoice?cfg=UNITCOST", CorpNum, UserID);

            return response.unitCost;
        }

        //과금정보 확인
        public ChargeInfo GetChargeInfo(string CorpNum, string UserID = null)
        {
            ChargeInfo response = httpget<ChargeInfo>("/Taxinvoice/ChargeInfo", CorpNum, UserID);

            return response;
        }

        #endregion

        [DataContract]
        private class IssueRequest
        {
            [DataMember] public string memo;
            [DataMember] public string emailSubject;
            [DataMember] public bool forceIssue = false;
        }

        [DataContract]
        private class MemoRequest
        {
            [DataMember] public string memo;
            [DataMember] public string emailSubject;
        }

        [DataContract]
        private class ResendRequest
        {
            [DataMember] public string receiver;
            [DataMember] public string sender = null;
            [DataMember] public string contents = null;
        }

        [DataContract]
        private class DocRequest
        {
            [DataMember] public int ItemCode;
            [DataMember] public string MgtKey;
        }

        [DataContract]
        public class CertResponse
        {
            [DataMember] public string certificateExpiration;
        }
    }
}