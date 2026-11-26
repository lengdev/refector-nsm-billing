using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NSMSOLUTION.Controllers
{
    [Authorize]
    public class NECController : Controller
    {
        // GET: NEC
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Get(string id)
        {
            using (var client = new HttpClient())
            {
                string necUrl = "https://voterlist.nec.gov.kh/";
                string vaccineUrl = "https://api.vaccine.gov.kh/citizen/search/nationalidcard";

                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("id_no", id.Trim()),
                    new KeyValuePair<string, string>("Activetab", "3"),
                    new KeyValuePair<string, string>("province", ""),
                    new KeyValuePair<string, string>("fname", ""),
                    new KeyValuePair<string, string>("lname", ""),
                    new KeyValuePair<string, string>("search_type", ""),
                    new KeyValuePair<string, string>("cardid", ""),
                    new KeyValuePair<string, string>("btnSearchId2", "")
                });
                formData.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                try
                {
                    HttpResponseMessage necResponse = await client.PostAsync(necUrl, formData);
                    necResponse.EnsureSuccessStatusCode();
                    string necHtml = await necResponse.Content.ReadAsStringAsync();

                    string pattern = @"<tr>\s*<td class=""center"">(\d+)</td>\s*<td class=""center"">([\d-]+)</td>\s*<td class=""center"">([^<]+)</td>\s*<td class=""center"">([^<]+)</td>\s*<td class=""tleft"">([^<]+)</td>\s*<td class=""center"">([^<]+)</td>\s*<td class=""center"">([\d-]+)</td>\s*<td class=""center"">\((\d+)\) ([^<]+)</td>\s*<td class=""tleft"">\((\d+)\) ([^<]+)</td>\s*<td class=""tleft"">\((\d+)\) ([^<]+)</td>";

                    var match = Regex.Match(necHtml, pattern);
                    UserInformation userInfo = null;
                    string inputDob = "";

                    if (match.Success)
                    {
                        string no = match.Groups[1].Value;
                        string _id = match.Groups[2].Value;
                        string name = match.Groups[5].Value;
                        var nameParts = name.Split(' ');
                        string firstName = nameParts.Length > 0 ? nameParts[0] : "";
                        string lastName = nameParts.Length > 1 ? nameParts[1] : "";
                        string gender = match.Groups[6].Value;
                        inputDob = match.Groups[7].Value;
                        DateTime dob = DateTime.ParseExact(inputDob, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                        userInfo = new UserInformation
                        {
                            No = match.Groups[1].Value,
                            ID = match.Groups[2].Value,
                            DocumentType = match.Groups[3].Value,
                            SecretCode = match.Groups[4].Value,
                            Identification = id,
                            FirstNameLastName = match.Groups[5].Value,
                            FirstName = firstName,
                            FirstNameL = UserInformation.KhmerToLatin(firstName),
                            LastName = lastName,
                            LastNameL = UserInformation.KhmerToLatin(lastName),
                            Gender = match.Groups[6].Value,
                            DateOfBirth = match.Groups[7].Value,//dob.ToString("yyyy-MM-dd"),
                            //City = match.Groups[8].Value,
                            Province = "(" + match.Groups[8].Value + ") " + match.Groups[9].Value,
                            Commune = "(" + match.Groups[10].Value + ") " + match.Groups[11].Value,
                            PollingStation = "(" + match.Groups[12].Value + ") " + match.Groups[13].Value
                        };
                    }

                    var payload = new { mandatory = id.Trim(), secondary = inputDob };
                    var vaccineContent = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                    VaccineRecord vaccineRecord = null;
                    HttpResponseMessage vaccineResp = await client.PostAsync(vaccineUrl, vaccineContent);
                    if (vaccineResp.IsSuccessStatusCode)
                    {
                        string vaccineJson = await vaccineResp.Content.ReadAsStringAsync();
                        if (!vaccineJson.Contains("NOT_FOUND"))
                        {
                            vaccineRecord = JsonConvert.DeserializeObject<VaccineRecord>(vaccineJson);

                            if (vaccineRecord?.NameEn != null)
                            {
                                var parts = vaccineRecord.NameEn.Split(' ');
                                vaccineRecord.FirstName = parts[0];
                                vaccineRecord.LastName = parts[1];
                                vaccineRecord.NameEn = $"{vaccineRecord.FirstName} {vaccineRecord.LastName}";

                                if (userInfo == null) userInfo = new UserInformation();
                                userInfo.FirstNameL = vaccineRecord.FirstName;
                                userInfo.LastNameL = vaccineRecord.LastName;
                                userInfo.VaccineRecord = vaccineRecord;
                            }

                            vaccineRecord.NationalId = id;
                        }
                        else if (userInfo == null)
                        {
                            return Json(new { error = "មិនមានឈ្មោះក្នុងបញ្ជីបោះឆ្នោត និងវ៉ាក់សាំង។" }, JsonRequestBehavior.AllowGet);
                        }
                    }

                    return Json(userInfo, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public class UserInformation
        {
            public string No { get; set; }
            public string ID { get; set; }
            public string DocumentType { get; set; }
            public string Identification { get; set; }
            public string SecretCode { get; set; }
            public string FirstNameLastName { get; set; }
            public string FirstName { get; set; }
            public string FirstNameL { get; set; }
            public string LastName { get; set; }
            public string LastNameL { get; set; }
            public string Gender { get; set; }
            public int GenderID { get; set; }
            public string DateOfBirth { get; set; }
            public string City { get; set; }
            public string Province { get; set; }
            public string Commune { get; set; }
            public string PollingStation { get; set; }
            public VaccineRecord VaccineRecord { get; set; }

            public static string KhmerToLatin(string khmer) => khmer; // Stub: Replace with real logic
            public static string CapitalizeFirstLetter(string word) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word);
        }
        public class VaccinationLabel
        {
            public string En { get; set; }
            public string Km { get; set; }
        }
        public class VaccineRecord
        {
            public string NameKH { get; set; }
            public string NameEn { get; set; }
            public string NationalId { get; set; }
            public string VacineName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FirstInjection { get; set; }
            public string SecondInjection { get; set; }
            public string FirstBoosterVaccineName { get; set; }
            public string ThirdInjection { get; set; }
            public string SecondBoosterVaccineName { get; set; }
            public string FourthInjection { get; set; }
            public string PhotoUrl { get; set; }
            public string OnBoardUrl { get; set; }
            public bool AlreadyOnBoard { get; set; }
            public string ChangeRequestUrl { get; set; }
            public string Hash { get; set; }
            public string DvcHash { get; set; }
            public bool IsFromMoD { get; set; }
            public string ModCertificateUrl { get; set; }
            public List<Vaccination> Vaccinations { get; set; }
        }

        public class Vaccination
        {
            public string Date { get; set; }
            public VaccinationLabel Label { get; set; }
            public string Vaccine { get; set; }
        }
    }
}