using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace jira_leadtime_calculator
{
    public class GoogleSheetService
    {
        private readonly string _sheetName = "Lead Time";
        private readonly string _applicationName = "Jira Lead Time Reporting";
        protected readonly string _spreadsheetId = "";
        private readonly GoogleCredential _credential;
        private readonly string _credentialPath = "";
        protected SheetsService _sheetsService;

        public GoogleSheetService()
        {
            _credential = GoogleCredential.FromFile(_credentialPath).CreateScoped(SheetsService.Scope.Spreadsheets);
            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = _applicationName,
            });
        }

        public async Task WriteIssuesToSheet(List<LeadTimeData> issues)
        {
            var requestBody = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request>()
            };

            // so first we clean out the existing data
            var clearRequest = _sheetsService.Spreadsheets.Values.Clear(
                new ClearValuesRequest(),
                _spreadsheetId,
                $"{_sheetName}!A2:Z");

            await clearRequest.ExecuteAsync();

            var sheetId = GetSheetId();

            foreach (var issue in issues)
            {
                // request to insert a row
                requestBody.Requests.Add(new Request
                {
                    InsertDimension = new InsertDimensionRequest
                    {
                        Range = new DimensionRange
                        {
                            SheetId = sheetId,
                            Dimension = "ROWS",
                            StartIndex = 1,  // 0-based index, so 1 is the second row
                            EndIndex = 2     // Insert 1 row (end is exclusive)
                        },
                        InheritFromBefore = false
                    }
                });

                // Request to update the newly inserted row with data
                var rowData = new RowData
                {
                    Values = new List<CellData>
                    {
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = issue.JiraIssueKey } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = issue.Summary } },
                        new CellData { UserEnteredValue = new ExtendedValue { StringValue = issue.CurrentStatus } },
                        CreateDateCell(issue.DateCreated),
                        CreateDateCell(issue.DateMovedToInProgress),
                        CreateDateCell(issue.DateMovedToInReview),
                        CreateDateCell(issue.DateMovedToReadyToTest),
                        CreateDateCell(issue.DateMovedToInTest),
                        CreateDateCell(issue.DateMovedToReadyToRelease),
                        CreateDateCell(issue.DateResolved),
                        new CellData { UserEnteredValue = new ExtendedValue { NumberValue = issue.TotalLeadTimeDays } },
                    }
                };

                // Request to update the newly inserted row with data
                requestBody.Requests.Add(new Request
                {
                    UpdateCells = new UpdateCellsRequest
                    {
                        Start = new GridCoordinate
                        {
                            SheetId = sheetId,
                            RowIndex = 1,   // Second row (0-based index)
                            ColumnIndex = 0  // First column (0-based index)
                        },
                        Rows = new List<RowData> { rowData },
                        Fields = "userEnteredValue,userEnteredFormat.numberFormat"
                    }
                });
            }

            // Execute the batch update
            var batchUpdateRequest = _sheetsService.Spreadsheets.BatchUpdate(requestBody, _spreadsheetId);
            var batchUpdateResponse = batchUpdateRequest.Execute();
        }

        private int GetSheetId()
        {
            var spreadsheet = _sheetsService.Spreadsheets.Get(_spreadsheetId).Execute();
            var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.Title == _sheetName);

            if (sheet == null)
            {
                throw new Exception($"Sheet '{_sheetName}' not found in spreadsheet.");
            }

            return sheet.Properties.SheetId.Value;
        }

        private CellData CreateDateCell(DateTime? date)
        {
            if(date == null)
            {
                return new CellData
                {
                    UserEnteredValue = new ExtendedValue
                    {
                        StringValue = string.Empty
                    }
                };
            }

            return CreateDateCell(date.Value);
        }

        private CellData CreateDateCell(DateTime date)
        {
            return new CellData
            {
                UserEnteredValue = new ExtendedValue
                {
                    // Convert DateTime to Google Sheets serial number
                    NumberValue = (date - new DateTime(1899, 12, 30)).TotalDays
                },
                UserEnteredFormat = new CellFormat
                {
                    NumberFormat = new NumberFormat
                    {
                        Type = "DATE",
                        Pattern = "yyyy-MM-dd"
                    }
                }
            };
        }
    }
}
