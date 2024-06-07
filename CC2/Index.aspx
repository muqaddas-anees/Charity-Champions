<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="CC2.Index" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Charity Champions</title>
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.1/themes/base/jquery-ui.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container my-3">
            <div class="row mb-3 justify-content-end">
                <div class="col-auto">
                    <div class="input-group">
                        <div class="input-group-prepend">
                            <span class="input-group-text">From Date</span>
                        </div>
                        <asp:TextBox ID="fromDate" runat="server" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-auto">
                    <div class="input-group">
                        <div class="input-group-prepend">
                            <span class="input-group-text">To Date</span>
                        </div>
                        <asp:TextBox ID="toDate" runat="server" CssClass="form-control" />
                    </div>
                </div>
                <div class="col-auto">
                    <asp:Button ID="exportButton" runat="server" Text="Export Transactions" CssClass="btn btn-dark" OnClick="ExportButton_Click" />
                </div>
                <div class="col-auto">
                    <button type="button" id="openAddCharityChampionModalButton" class="btn btn-primary">Add New Charity Champion</button>
                </div>
                <div class="col-auto">
                    <div class="input-group my-3">
                        <input type="text" id="searchInput" class="form-control" placeholder="Search by First Name, Last Name, or Referral Code" />
                        <div class="input-group-append">
                            <button id="searchButton" class="btn btn-primary" type="button" onclick="searchCharityChampions()">Search</button>
                        </div>
                    </div>
                </div>
            </div>
            <asp:GridView ID="charityChampionsGridView" runat="server" CssClass="table table-striped table-bordered" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="FirstName" HeaderText="First Name" />
                    <asp:BoundField DataField="LastName" HeaderText="Last Name" />
                    <asp:BoundField DataField="ReferralCode" HeaderText="Referral Code" />
                    <asp:HyperLinkField DataTextField="RegistrationLink" HeaderText="Registration Link" DataNavigateUrlFields="RegistrationLink" />
                    <asp:BoundField DataField="Commission" HeaderText="% Commission" />
                    <asp:BoundField DataField="NoOfInstances" HeaderText="No. of Instances" />
                    <asp:BoundField DataField="DonationsThisMonth" HeaderText="Donations This Month" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="DonationsLastMonth" HeaderText="Donations Last Month" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="DonationsThisYear" HeaderText="Donations This Year" DataFormatString="{0:C}" />
                </Columns>
            </asp:GridView>
        </div>

        <!-- Modal for adding new Charity Champion -->
        <div class="modal" id="addCharityChampionModal" tabindex="-1" role="dialog" aria-labelledby="addCharityChampionModalLabel" aria-hidden="true">
            <div class="modal-dialog" role="document">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="addCharityChampionModalLabel">Add New Charity Champion</h5>
                    </div>
                    <div class="modal-body">
                        <div class="form-group">
                            <label for="firstName">First Name</label>
                            <asp:TextBox ID="firstNameTextBox" runat="server" CssClass="form-control" />
                        </div>
                        <div class="form-group">
                            <label for="lastName">Last Name</label>
                            <asp:TextBox ID="lastNameTextBox" runat="server" CssClass="form-control" />
                        </div>
                        <div class="form-group">
                            <label for="commission">Commission %</label>
                            <asp:TextBox ID="commissionTextBox" runat="server" CssClass="form-control" onkeypress="return isNumberKey(event)" />
                        </div>

                    </div>
                    <div class="modal-footer">
                        <asp:Button ID="saveCharityChampionButton" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="SaveCharityChampionButton_Click" UseSubmitBehavior="false" AutoPostBack="false" />
                        <button type="button" class="btn btn-secondary" onclick="$('#addCharityChampionModal').modal('hide')">Close</button>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script src="Scripts/jquery-3.7.0.min.js"></script>
    <script src="Scripts/bootstrap.bundle.min.js"></script>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://code.jquery.com/ui/1.13.1/jquery-ui.min.js"></script>
    <script>
        $(function () {
            $("#<%= fromDate.ClientID %>").datepicker({
                dateFormat: "dd-mm-yy",
                changeMonth: true,
                changeYear: true
            });
        });
        $(function () {
            $("#<%= toDate.ClientID %>").datepicker({
                dateFormat: "dd-mm-yy",
                changeMonth: true,
                changeYear: true
            });
        });

        function isNumberKey(evt) {
            var charCode = (evt.which) ? evt.which : evt.keyCode;
            var value = document.getElementById("<%= commissionTextBox.ClientID %>").value;

            // Allow backspace, delete, tab, and enter keys
            if (charCode == 8 || charCode == 9 || charCode == 13) {
                return true;
            }
            // Allow only digits and ensure the value stays between 0 and 100
            if (charCode >= 48 && charCode <= 57 && parseInt(value + String.fromCharCode(charCode)) <= 100) {
                return true;
            }
            return false;
        }

        $(function () {
            $('#openAddCharityChampionModalButton').click(function () {
                $('#addCharityChampionModal').modal('show');
                // Clear form fields when modal is opened
                $('#<%= firstNameTextBox.ClientID %>').val('');
                $('#<%= lastNameTextBox.ClientID %>').val('');
                $('#<%= commissionTextBox.ClientID %>').val('');
            });
        });

        function searchCharityChampions() {
            var input, filter, table, tr, td, i, txtValue;
            input = document.getElementById("searchInput");
            filter = input.value.toUpperCase();
            table = document.getElementById("charityChampionsGridView");
            tr = table.getElementsByTagName("tr");

            for (i = 0; i < tr.length; i++) {
                td = tr[i].getElementsByTagName("td");
                for (var j = 0; j < td.length; j++) {
                    if (td[j]) {
                        txtValue = td[j].textContent || td[j].innerText;
                        if (txtValue.toUpperCase().indexOf(filter) > -1) {
                            tr[i].style.display = "";
                            break;
                        } else {
                            tr[i].style.display = "none";
                        }
                    }
                }
            }
        }
    </script>
</body>
</html>
