using System.Web.Optimization;

namespace NewInventorySystem
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.form.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/AdminLTE/css").Include(
                "~/Content/AdminLTE-3.2.0/dist/css/adminlte.min.css",
                "~/Content/bootstrap-glyphicons.css",
                "~/Content/AdminLTE-3.2.0/plugins/fontawesome-free/css/all.min.css",
                "~/Content/AdminLTE-3.2.0/plugins/overlayScrollbars/css/OverlayScrollbars.min.css",
                "~/Content/AdminLTE-3.2.0/dist/css/material-dashboard.css",
                "~/Content/AdminLTE-3.2.0/dist/css/ionicons.min.css",
                "~/Content/AdminLTE-3.2.0/dist/css/icheck-material.min.css",
                "~/Content/AdminLTE-3.2.0/plugins/sweetalert2/sweetalert2.min.css",
                "~/Content/AdminLTE-3.2.0/plugins/sweetalert2-theme-bootstrap-4/bootstrap-4.min.css",
                "~/Content/AdminLTE-3.2.0/plugins/toastr/toastr.min.css",
                //"~/Content/AdminLTE-3.2.0/plugins/select2/css/select2.min.css",
                //"~/Content/AdminLTE-3.2.0/plugins/select2-bootstrap4-theme/select2-bootstrap4.css",
                "~/Content/AdminLTE-3.2.0/plugins/daterangepicker/daterangepicker.css",
                "~/Content/AdminLTE-3.2.0/plugins/icheck-bootstrap/icheck-bootstrap.min.css",
                "~/Content/AdminLTE-3.2.0/plugins/bootstrap-colorpicker/css/bootstrap-colorpicker.min.css",
                "~/Content/AdminLTE-3.2.0/plugins/bootstrap-datetimepicker/bootstrap-datepicker.css",
                "~/Content/AdminLTE-3.2.0/plugins/tempusdominus-bootstrap-4/css/tempusdominus-bootstrap-4.min.css",
                "~/Content/TableGrid/Style/TableStyle.css",
                "~/Scripts/jquery-filestyle.min.css",
                "~/Content/Style/css-file-icons.css",
                "~/Content/Style/baguetteBox.min.css",
                //"~/Scripts/tooltipster-master/dist/css/tooltipster.bundle.css",
                //"~/Scripts/tooltipster-master/dist/css/plugins/tooltipster/sideTip/themes/tooltipster-sideTip-punk.min.css",
                //"~/Scripts/tooltipster-master/dist/css/plugins/tooltipster/sideTip/themes/tooltipster-sideTip-light.min.css",
                "~/Scripts/bootstrap-datepicker/css/bootstrap-datepicker.standalone.css",
                "~/Content/bootstrapfileinput/css/fileinput.css",
                "~/Content/bootstrapfileinput/themes/explorer-fas/theme.css",
                "~/Content/loading-bar.min.css",
                "~/Content/ng-table.min.css",
                "~/Content/TableGrid/Style/TableStyle.css",
                "~/Content/css/step.css",
                "~/Content/ContactV2_files/main.css",
                "~/Content/AdminLTE-3.2.0/plugins/fontawesome-free/css/font-awesome.min.css"
            ));
            bundles.Add(new ScriptBundle("~/AdminLTE/script").Include(
                "~/Content/AdminLTE-3.2.0/dist/js/popper.js",
                "~/Content/AdminLTE-3.2.0/plugins/bootstrap/js/bootstrap.js",
                "~/Content/AdminLTE-3.2.0/plugins/bootstrap/js/bootstrap.bundle.min.js",
                "~/Content/AdminLTE-3.2.0/dist/js/tooltip.js",
                "~/Content/AdminLTE-3.2.0/plugins/overlayScrollbars/js/jquery.overlayScrollbars.min.js",
                "~/Content/AdminLTE-3.2.0/dist/js/adminlte.js",
                "~/Content/AdminLTE-3.2.0/plugins/jquery-mousewheel/jquery.mousewheel.js",
                "~/Content/AdminLTE-3.2.0/plugins/toastr/toastr.min.js",
                "~/Content/AdminLTE-3.2.0/plugins/moment/moment.min.js",
                "~/Content/AdminLTE-3.2.0/plugins/bootstrap-datetimepicker/bootstrap-datepicker.js",
                "~/Content/AdminLTE-3.2.0/plugins/daterangepicker/daterangepicker.js",
                "~/Content/AdminLTE-3.2.0/plugins/select2/js/select2.full.min.js",
                "~/Content/AdminLTE-3.2.0/plugins/overlayScrollbars/js/jquery.overlayScrollbars.min.js",
                "~/Content/AdminLTE-3.2.0/plugins/datatables/jquery.dataTables.min.js",
                "~/Content/AdminLTE-3.2.0/plugins/datatables-bs4/js/dataTables.bootstrap4.min.js",
                "~/Content/AdminLTE-3.2.0/plugins/datatables-responsive/js/dataTables.responsive.min.js",
                "~/Content/AdminLTE-3.2.0/plugins/datatables-responsive/js/responsive.bootstrap4.min.js",
                "~/Content/AdminLTE-3.2.0/plugins/bootstrap-switch/js/bootstrap-switch.js",
                "~/Content/AdminLTE-3.2.0/plugins/sweetalert2/sweetalert2.min.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/script").Include(
                        //"~/Content/sweetalert/dist/sweetalert2.all.js",
                        "~/Scripts/JScript_Function.js",
                        "~/Scripts/jquery-ui.js",
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/additional-methods.js",
                        "~/Scripts/tooltipster-master/dist/js/tooltipster.bundle.js",
                        "~/Scripts/jquery-filestyle.min.js",
                        "~/Scripts/typeahead.bundle.js",
                        "~/Scripts/accounting/accounting.js",
                        "~/Scripts/jquery.format.js",
                        "~/Scripts/AutoNumeric.js",
                        "~/Scripts/printThis.js",
                        "~/Scripts/jQuery.print.js",
                        "~/Scripts/jquery.serialize-object.min.js"

            //,
            //,
            //
            //"~/Scripts/jquery/jquery.validate.js",
            //"~/Scripts/jquery/additional-methods.js",
            //"~/Scripts/tooltipster-master/dist/js/tooltipster.bundle.js",
            //"~/Scripts/jquery-ui-1.12.1/jquery-ui.js",
            //"~/Scripts/bootstrap-datepicker/js/bootstrap-datepicker.js",
            //"~/Scripts/baguetteBox.min.js",
            //"~/Content/bootstrapfileinput/js/plugins/piexif.js",
            //"~/Content/bootstrapfileinput/js/plugins/sortable.js",
            //"~/Content/bootstrapfileinput/js/fileinput.js",
            //"~/Content/bootstrapfileinput/themes/fas/theme.js",
            //"~/Content/bootstrapfileinput/themes/explorer-fas/theme.js",
            //"~/Scripts/sweetalert.min.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/angularjsAdmin").Include(
                        "~/Scripts/angular.min.js",
                        "~/Scripts/angular-sanitize.min.js",
                        "~/Scripts/angular-route.min.js",
                        "~/Scripts/angular-cookies.min.js",
                        "~/Scripts/ngStorage.min.js",
                        "~/Scripts/ng-table.js",
                        "~/Scripts/restangular.min.js",
                       "~/Scripts/underscore-min.js",
                       "~/Scripts/loading-bar.min.js",
                        "~/Scripts/angular-local-storage.min.js",
                        "~/Scripts/angular-animate.min.js",
                        "~/Scripts/angular-filter.min.js",
                        "~/Scripts/angular-underscore.min.js",
                        "~/App/JScriptForm.js",
                        "~/Scripts/LocalStorage.js",
                        "~/Scripts/ngHtmlCompile.js",
                        "~/Scripts/ui-bootstrap-tpls-1.3.1.js",
                        "~/App/app.js",
                        "~/App/appAdmin.js",
                        "~/App/Directive/activeNav.js",
                        "~/App/Directive/breacrumb.js",
                        "~/App/Services/AuthenticationService.js",
                        "~/App/Services/AuthData.js",
                        "~/App/Services/LoginService.js",
                        "~/App/Controllers/Administrator/DashboardController.js",
                        "~/App/Controllers/Administrator/CreateWarehouseController.js",
                        "~/App/Controllers/Administrator/CreatePagePermissionController.js",
                        "~/App/Controllers/Administrator/CreateSystemController.js",
                        "~/App/Controllers/Administrator/CreateUserController.js",
                        "~/App/Controllers/Administrator/SetUserPermissionController.js",
                        "~/App/Controllers/Administrator/CreateUserLoginTmpController.js"

            ));

            bundles.Add(new ScriptBundle("~/bundles/angularjsUser").Include(
                        "~/Scripts/angular.min.js",
                        "~/Scripts/angular-sanitize.min.js",
                        "~/Scripts/angular-route.min.js",
                        "~/Scripts/ng-breadcrumbs.js",
                        "~/Scripts/angular-cookies.min.js",
                        "~/Scripts/ngStorage.min.js",
                        "~/Scripts/ng-table.js",
                        "~/Scripts/restangular.min.js",
                        "~/Scripts/underscore-min.js",
                        "~/Scripts/loading-bar.min.js",
                        "~/Scripts/angular-local-storage.min.js",
                        "~/Scripts/angular-animate.min.js",
                        "~/Scripts/angular-filter.min.js",
                        //"~/Scripts/angular-underscore.min.js",
                        "~/Scripts/LocalStorage.js",
                        "~/Scripts/ngHtmlCompile.js",
                        "~/Scripts/ui-bootstrap-tpls-1.3.1.js",
                        "~/App/app.js",
                        "~/App/appHome.js",
                        "~/App/JScriptForm.js",
                        "~/App/Directive/activeNav.js",
                        "~/App/Directive/breacrumb.js",
                        "~/App/Services/AuthData.js",
                        "~/App/Services/AuthenticationService.js",
                        "~/App/Services/LoginService.js",
                        "~/App/Controllers/HomeController.js",
                        "~/App/Controllers/AddExpenseTypeController.js",
                        "~/App/Controllers/AddDepartmentController.js",
                        "~/App/Controllers/AddPositionController.js",
                        "~/App/Controllers/AddWorkingTimeController.js",
                        "~/App/Controllers/AddCompanyController.js",
                        "~/App/Controllers/AddEmployeesController.js",
                        "~/App/Controllers/AddServicesController.js",
                        "~/App/Controllers/AddConnectionTypeController.js",
                        "~/App/Controllers/AddPackagesController.js",
                        "~/App/Controllers/AddPackageDurationController.js",
                        "~/App/Controllers/AddCustomerTypeController.js",
                        "~/App/Controllers/AddOrderTypeController.js",
                        "~/App/Controllers/AddPaymentTermsController.js",
                        "~/App/Controllers/AddIndustryTypeController.js",
                        "~/App/Controllers/AddOtherChargeController.js",
                        "~/App/Controllers/AddStockNameController.js",
                        "~/App/Controllers/AddSupplierController.js",
                        "~/App/Controllers/AddItemGroupController.js",
                        "~/App/Controllers/AddItemController.js",
                        "~/App/Controllers/AddCustomerController.js",
                        "~/App/Controllers/AddProvinceCityController.js",
                        "~/App/Controllers/AddDistrictController.js",
                        "~/App/Controllers/AddCommuneController.js",
                        "~/App/Controllers/AddTermConditionController.js",
                        "~/App/Controllers/CustomerProfileController.js",
                        "~/App/Controllers/AddUserLoginController.js",
                        "~/App/Controllers/Administrator/PagenotfoundController.js",
                        "~/App/Controllers/AddWebMenuController.js",
                        "~/App/Controllers/AddWebSlideShowController.js",
                        "~/App/Controllers/AddAccountController.js",
                        "~/App/Controllers/AddPaymentCategoryController.js",
                        "~/App/Controllers/AddPaymentMethodController.js"

            ));
            bundles.Add(new ScriptBundle("~/bundles/angularjsDashboard").Include(
                        "~/Scripts/angular.min.js",
                        "~/Scripts/angular-sanitize.min.js",
                        "~/Scripts/angular-route.min.js",
                        "~/Scripts/ng-breadcrumbs.js",
                        "~/Scripts/angular-cookies.min.js",
                        "~/Scripts/ng-table.js",
                        "~/Scripts/restangular.min.js",
                        "~/Scripts/underscore-min.js",
                        "~/Scripts/loading-bar.min.js",
                        "~/Scripts/angular-local-storage.min.js",
                        "~/Scripts/angular-animate.min.js",
                        "~/App/appLogin.js",
                        "~/App/JScriptForm.js",
                        "~/App/Directive/activeNav.js",
                        "~/App/Directive/breacrumb.js",
                        "~/App/Services/AuthenticationService.js",
                        "~/App/Services/AuthData.js",
                        "~/App/Services/LoginService.js",
                        "~/App/Controllers/DashboardController.js"
            ));
            bundles.Add(new ScriptBundle("~/bundles/angularjsLogin").Include(
                        "~/Scripts/angular.min.js",
                        "~/Scripts/angular-sanitize.min.js",
                        "~/Scripts/angular-route.min.js",
                        "~/Scripts/ng-breadcrumbs.js",
                        "~/Scripts/angular-cookies.min.js",
                        "~/Scripts/ng-table.js",
                        "~/Scripts/restangular.min.js",
                        "~/Scripts/underscore-min.js",
                        "~/Scripts/loading-bar.min.js",
                        "~/Scripts/angular-local-storage.min.js",
                        "~/Scripts/angular-animate.min.js",
                        "~/App/appLogin.js",
                        //"~/App/app.js",
                        "~/App/JScriptForm.js",
                        "~/App/Directive/activeNav.js",
                        "~/App/Directive/breacrumb.js",
                        "~/App/Services/AuthenticationService.js",
                        "~/App/Services/AuthData.js",
                        "~/App/Services/LoginService.js"
            ));
        }
    }
}
