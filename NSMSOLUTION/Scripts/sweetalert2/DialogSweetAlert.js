function DialogSweetAlert(type, Message, CallBackFunction) {
    CallBackFunction = (typeof CallBackFunction === "undefined") ? "" : CallBackFunction;
    Message = (typeof Message === 'undefined') ? "" : Message;

    Swal.close();

    switch (type) {
        case 'load':
            if (Message === "") Message = "Loading...";
            Swal.fire({
                position: 'top',
                title: Message,
                allowEscapeKey: false,
                allowOutsideClick: false,
                showConfirmButton: false,
                willOpen: () => Swal.showLoading(),
                didOpen: () => {
                    if (typeof CallBackFunction === 'function') {
                        setTimeout(CallBackFunction, 500);
                    }
                },
                willClose: () => Swal.hideLoading(),
                didClose: () => {
                    document.body.style.overflow = 'auto';
                }
            });
            break;

        case 'error':
            $("#DialogMessageBox").remove();
            if (Message === "") Message = 'Something went wrong!';
            Swal.fire({
                //type: "error",
                position: 'top',
                icon: 'error',
                title: 'Oops...',
                html: Message,
                showConfirmButton: true,
                //animation: !1,
                //customClass: "animated bounceIn",
                //confirmButtonClass: "btn btn-primary",
                //buttonsStyling: !1,
                didClose: () => {
                    if (typeof CallBackFunction === 'function') {
                        CallBackFunction();
                    }
                },
                willClose: () => Swal.hideLoading()
            });
            break;

        case 'deleteted':
            if (Message === "") Message = 'Your data has been deleted.';
            Swal.fire({
                position: 'top',
                icon: 'success',
                title: "Deleted!",
                html: Message,
                showConfirmButton: false,
                allowOutsideClick: false,
                timer: 1000,
                willClose: () => Swal.hideLoading()
            });
            break;

        case 'Savesuccess':
            if (Message === "") Message = 'Data save change successful.';
            toastr.options = {
                closeButton: false,
                debug: false,
                newestOnTop: true,
                progressBar: true,
                positionClass: "toast-top-right",
                preventDuplicates: false,
                showDuration: "300",
                hideDuration: "1000",
                timeOut: "1500",
                extendedTimeOut: "1000",
                showEasing: "swing",
                hideEasing: "linear",
                showMethod: "fadeIn",
                hideMethod: "fadeOut"
            };
            toastr.success(Message);

            setTimeout(() => {
                $("#DialogMessageBox").remove();
                Swal.hideLoading();
                Swal.close();
                toastr.remove();
                $(".swal2-container").remove();
                $("#toast-container").remove();
                $('button').removeAttr('disabled');
                if (typeof CallBackFunction === 'function') {
                    CallBackFunction();
                }
            }, 800);
            break;

        case 'success':
            if (Message === "") Message = 'Your work has been saved';
            Swal.fire({
                position: 'top',
                icon: 'success',
                title: Message,
                showConfirmButton: false,
                allowOutsideClick: false,
                timer: 1000
            }).then(() => {
                DialogSweetAlert("load", "Please wait...");
            });
            break;

        case 'loadsuccess':
            if (Message === "") Message = 'Your data has been loaded.';
            Swal.fire({
                position: 'top',
                icon: 'success',
                title: Message,
                showConfirmButton: false,
                allowOutsideClick: false,
                allowEscapeKey: false,
                timer: 1000,
                willClose: () => {
                    $('button').removeAttr('disabled');
                    Swal.hideLoading();
                }
            });
            break;

        case 'message':
            $("#DialogMessageBox").remove();
            if (Message === "") Message = 'Something went wrong!';
            Swal.fire({
                position: 'top',
                icon: 'info',
                html: Message,
                showConfirmButton: true,
                willClose: () => Swal.hideLoading()
            });
            break;

        case 'loginsuccess':
            Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 2000,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer);
                    toast.addEventListener('mouseleave', Swal.resumeTimer);
                }
            }).fire({
                icon: 'success',
                title: 'Signed in successfully'
            });
            break;

        case 'nopermission':
            if (Message === "") Message = "Something went wrong!";
            Swal.fire({
                position: 'top',
                icon: 'error',
                title: 'Oops...',
                html: Message,
                showConfirmButton: true
            });
            break;

        case 'close':
            $("#DialogMessageBox").remove();
            Swal.close();
            break;

        case 'SignInsuccess':
            if (Message === "") Message = 'Signed in successfully';
            Swal.fire({
                position: 'top',
                icon: 'success',
                title: Message,
                showConfirmButton: false,
                allowOutsideClick: false,
                timer: 1000,
                heightAuto: false
            }).then(() => {
                DialogSweetAlert("load", "Please wait...");
            });
            break;

        case 'ToastrLoading':
            if (Message === "") Message = 'Loading...';
            toastr.options = {
                closeButton: false,
                debug: false,
                newestOnTop: true,
                progressBar: true,
                positionClass: "toast-top-center",
                preventDuplicates: true,
                showDuration: "300",
                hideDuration: "1000",
                timeOut: "1500",
                extendedTimeOut: "1000",
                showEasing: "swing",
                hideEasing: "linear",
                showMethod: "fadeIn",
                hideMethod: "fadeOut"
            };
            toastr.info(Message);
            break;
    }
}
