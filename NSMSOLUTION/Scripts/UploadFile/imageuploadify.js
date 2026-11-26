/*!
 * Imageuploadify - jQuery plugin
 * Allow to change input file to a box allowing drag'n drop and preview images before
 * updloading them.
 */

// Semi-colon to protect against concatened scripts, etc...
// Ensure that $ is referencing to jQuery.
// window and document to slightly quicken the process.
// To be sure that undefined is truly undefined (For ES3)
var IsShowdrawPhotoEditor = true;
var GetdTData = new DataTransfer();
(function ($, window, document, undefined) {
  var $h = {
    compare: function (input, str, exact) {
      return input !== undefined && (exact ? input === str : input.match(str));
    },
  };
  var $image = function (vType, vName) {
    return (($h.compare(vType, "image.*") &&
      !$h.compare(vType, /(tiff?|wmf)$/i)) ||
      $h.compare(vName, /\.(gif|png|jpe?g)$/i)) == null
      ? false
      : ($h.compare(vType, "image.*") && !$h.compare(vType, /(tiff?|wmf)$/i)) ||
          $h.compare(vName, /\.(gif|png|jpe?g)$/i);
  };
  // Prevent issues about browser opening file by dropping it.
  window.addEventListener(
    "dragover",
    function (e) {
      e = e || event;
      e.preventDefault();
    },
    false,
  );
  window.addEventListener(
    "drop",
    function (e) {
      e = e || event;
      e.preventDefault();
    },
    false,
  );

  const compareMimeType = (mimeTypes, fileType, formatFile) => {
    // If accept is defined as *.
    if (mimeTypes.length < 2 && mimeTypes[0] === "*") {
      return true;
    }

    // Checking all types written in accept.
    for (let index = 1; index < mimeTypes.length; index += 3) {
      // image/*, audio/*, video/*
      if (
        mimeTypes[index + 1] === "*" &&
        fileType.search(new RegExp(mimeTypes[index])) != -1
      ) {
        return true;
      }
      // application/vnd.ms-excel, application/vnd.ms-powerpoint
      else if (
        mimeTypes[index + 1] &&
        mimeTypes[index + 1] != "*" &&
        fileType.search(new RegExp("\\*" + mimeTypes[index + 1] + "\\*")) != -1
      ) {
        return true;
      }
      // application/pdf, image/jpg
      else if (
        mimeTypes[index + 1] &&
        mimeTypes[index + 1] != "*" &&
        fileType.search(new RegExp(mimeTypes[index + 1])) != -1
      ) {
        return true;
      }
      // .jpg, .pdf .png
      else if (
        mimeTypes[index + 1] === "" &&
        (fileType.search(new RegExp(mimeTypes[index])) != -1 ||
          formatFile.search(new RegExp(mimeTypes[index])) != -1)
      ) {
        return true;
      }
    }
    return false;
  };

  // Define the plugin imageuploadify.
  $.fn.imageuploadify = function (opts) {
    // Override default option with user's if exist.
    const settings = $.extend({}, $.fn.imageuploadify.defaults, opts);

    // Initialize every element.

    this.each(function () {
      // Save the current element to self to avoid conflict.
      const self = this;

      // Apply on input file having "multiple" attribute only.
      if (!$(self).attr("multiple")) {
        return;
      }
      // Save accept files
      let accept = $(self).attr("accept")
        ? $(self).attr("accept").replace(/\s/g, "").split(",")
        : null;
      let result = [];

      // Loop the array of accept files to split all part of mimetype or format.
      accept.forEach((item) => {
        let regexp;
        // Select the regexp according to the result (mimetype or format)
        if (item.search(/\//) != -1) {
          regexp = new RegExp("([A-Za-z-.]*)\/([A-Za-z-*.]*)", "g");
        } else {
          regexp = new RegExp("\.([A-Za-z-]*)()", "g");
        }
        // Exec the regexp and then
        const r = regexp.exec(item);
        result = result.concat(r);
      });

      // Array containing all files add by dialog box or drag'n drop.
      let totalFiles = [];
      let pendingFileProcessing = Promise.resolve();
      // Count the number of time a "dragenter" enter the box.
      let counter = 0;

      // Define the dragbox layout.
      let dragbox = $(`
      <div class="imageuploadify well">
        <div class="imageuploadify-overlay">
        <i class="fa fa-picture-o"></i>
        </div>
        <div class="imageuploadify-images-list text-center">
          <i class="fa-regular fa-cloud-arrow-up"></i>
          <span class='imageuploadify-message'>Drag & Drop Your File(s) Here To Upload</span>
          <button type="button" class="btn btn-default">or select file to upload</button>
        </div>
      </div>
      `);

      // Save all elements of the dragbox.
      let overlay = dragbox.find(".imageuploadify-overlay");
      let uploadIcon = dragbox.find(".imageuploadify-overlay i");

      let imagesList = dragbox.find(".imageuploadify-images-list");
      let addIcon = dragbox.find(".imageuploadify-images-list i");
      let addMsg = dragbox.find(".imageuploadify-images-list span");
      let button = dragbox.find(".imageuploadify-images-list button");

      /** FUNCTIONS  **/

      // Function to read and store files.
      const getRoundedCanvas = function getRoundedCanvas(sourceCanvas) {
        var canvas = document.createElement("canvas");
        var context = canvas.getContext("2d");
        var width = sourceCanvas.width;
        var height = sourceCanvas.height;

        canvas.width = width;
        canvas.height = height;
        context.imageSmoothingEnabled = true;
        context.drawImage(sourceCanvas, 0, 0, width, height);
        context.globalCompositeOperation = "destination-in";
        context.beginPath();
        context.arc(
          width / 2,
          height / 2,
          Math.min(width, height) / 2,
          0,
          2 * Math.PI,
          true,
        );
        context.fill();
        return canvas;
      };

      const drawPhotoEditor = function (url, filename) {
        $("#EditPhoto").remove();

        var modal = `<div class="modal fade" tabindex="-1" role="dialog" id="EditPhoto">
                <div class="modal-dialog" tabindex="-1" role="dialog">
                    <div class="modal-content">
                        <div class="card-default">
                            <div class="card-header">
                                <i class="fas fa-edit"></i>&nbsp;Crop the image
                                <button type="button" class="close" data-dismiss="modal">
                                    <span>&times;</span>
                                </button>
                            </div>
                        </div>
                        <div class="modal-body" id="EditPhotomodalbody">
                            <div class="img-container">
                                <img id="img_cropping" style="max-height:500px" />
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" id="btnCrop" name="btnCrop" class="btn btn-outline-primary"><i class="fa fa-crop-alt"></i> Crop</button>
                            <button type="button" id="btnCloseID" name="btnCloseID" class="btn btn-outline-danger"><i class="glyphicon glyphicon-off"></i> Close</button>
                        </div>
                    </div>
                </div>
            </div>`;

        if ($("#EditPhoto").length == 0) $(document.body).append(modal);

        //$("#EditPhoto").modal({
        //    keyboard: false,
        //    backdrop: 'static',
        //    show: true
        //});
        let myModal = new bootstrap.Modal(
          document.getElementById("EditPhoto"),
          {
            keyboard: false,
            backdrop: "static",
            focus: true,
          },
        );
        myModal.show();

        let image = document.getElementById("img_cropping");
        image.src = url;
        let $modal = $("#EditPhoto");

        //let avatar = document.getElementById('profilePic');
        //let image = document.getElementById('img_cropping');
        //let input = document.getElementById('custFile');
        $modal.draggable({
          handle: $("#EditPhoto .card-default"),
        });
        $modal
          .on("shown.bs.modal", function () {
            $(".modal-backdrop").eq(1).remove();
            let image = document.getElementById("img_cropping");
            let cropper = new Cropper(image, {
              movable: true,
              //zoomable: false,
              //rotatable: false,
              //aspectRatio: 1,
              viewMode: 2,
              autoCropArea: 1,
              ready: function () {
                //croppable = true;
                cropper.setDragMode("move");
              },
              //ready: function (e) {

              //    //cropper.setDragMode("move");
              //}
            });
            $("#btnCloseID").click(function () {
              $("#EditPhoto").remove();
              $(".modal-backdrop").eq(1).remove();
            });
            $("#btnCrop").click(function () {
              var croppedCanvas;
              var roundedCanvas;
              var roundedImage;
              //croppedCanvas = cropper.getCroppedCanvas();
              //roundedCanvas = getRoundedCanvas(croppedCanvas);
              //roundedImage = document.createElement('img');
              //roundedImage.src = roundedCanvas.toDataURL();

              cropper.getCroppedCanvas().toBlob((blob) => {
                //let exec = /(?:\.([^.]+))?$/.exec(filename)[0];
                //filename = filename.replace(exec, Math.random().toString(36).substr(2, 9)) + exec;//moment().format('ss')
                const file = new File([blob], filename, { type: blob.type });
                let list = new DataTransfer();
                list.items.add(file);
                let myFileList = list.files;
                $("input[name=custFile]").prop("files", myFileList);
                queueFiles(myFileList);
                $("#EditPhoto").remove();
                $(".modal-backdrop").eq(1).remove();
                //$('input[name=custFile]').trigger('change');
              });
            });
          })
          .on("hidden.bs.modal", function () {
            $("#EditPhoto").remove();
            $(".modal-backdrop").eq(1).remove();
            if (cropper) {
              cropper.destroy();
              cropper = null;
            }
          });
      };

      const retrieveFiles = async (files) => {
        const selectedFiles = Array.prototype.slice.call(files);
        const preparedFiles = await Promise.all(
          selectedFiles.map(async (file) => {
            if (
              file.type &&
              file.type.indexOf("image/") === 0 &&
              window.nsmCompressImageFile
            ) {
              try {
                return await window.nsmCompressImageFile(file);
              } catch (error) {
                return file;
              }
            }
            return file;
          }),
        );

        for (let index = 0; index < preparedFiles.length; ++index) {
          let file = preparedFiles[index];
          const extension = /(?:\.([^.]+))?$/.exec(file.name)[1];
          if (!accept || compareMimeType(result, file.type, extension)) {
            const id = Math.random().toString(36).substr(2, 9);
            const originalName = file.name;
            const extensionMatch = /(?:\.([^.]+))?$/.exec(originalName);
            const suffix = extensionMatch[0];
            const filename =
              originalName.replace(
                suffix,
                "_" + Math.random().toString(36).substr(2, 5),
              ) + suffix;
            const formData = new FormData();
            formData.append("file", file, filename);
            file = formData.get("file");
            readingFile(id, file);
            totalFiles.push({ id: id, file: file });
          } else {
            DialogSweetAlert(
              "error",
              "File type not support for upload. Please upload only " + accept,
            );
          }
        }

        const dataTransfer = new DataTransfer();
        for (let index = 0; index < totalFiles.length; ++index) {
          dataTransfer.items.add(totalFiles[index].file);
        }
        self.files = dataTransfer.files;
      };

      const queueFiles = (files) => {
        pendingFileProcessing = pendingFileProcessing.then(() =>
          retrieveFiles(files),
        );
        return pendingFileProcessing;
      };

      setTimeout(function () {
        if (GetdTData.files.length > 0) {
          $(self).prop("files", GetdTData.files);
          queueFiles(GetdTData.files);
        }
      }, 800);
      // Function to read a file.
      const readingFile = (id, file) => {
        const fReader = new FileReader();

        // Compute the number of box that could fit in the dragbox and the
        // margin according to it.
        const width = dragbox.width();
        const boxesNb = Math.floor(width / 100);
        const marginSize = Math.floor((width - boxesNb * 100) / (boxesNb + 1));

        // Create the preview file container box.
        let container = $(`<div class='imageuploadify-container'>
          <button type='button' class='btn btn-danger fa-solid fa-circle-xmark'></button>
          <div class='imageuploadify-details'>
            <span title='${file.name}'>${file.name}</span>
          </div>
        </div>`);
        //<span>${file.type}</span>
        //<span>${file.size}</span>

        let details = container.find(".imageuploadify-details");
        let deleteBtn = container.find("button");

        // Preview file container box CSS
        container.css("margin-left", marginSize + "px");

        // Manage display/hidding details about preview files.
        //details.hover(function() {
        //  $(this).css("opacity", "1");
        //})
        //.mouseleave(function() {
        //  $(this).css("opacity", "0");
        //});

        // If the given file in the parameter is an image.
        if (file.type && file.type.search(/image/) != -1) {
          // Associated function to a ending load
          fReader.onloadend = function (e) {
            // Create the image tag for preview.
            let image = $("<img>");
            // Paste the image source to display the image preview.
            image.attr("src", e.target.result);

            // Append the image to its container and then the container to the
            // list of files.
            container.append(image);
            imagesList.append(container);

            // Apply left margin to first container of each row and right to last.
            imagesList
              .find(".imageuploadify-container:nth-child(" + boxesNb + "n+4)")
              .css("margin-left", marginSize + "px");
            imagesList
              .find(".imageuploadify-container:nth-child(" + boxesNb + "n+3)")
              .css("margin-right", marginSize + "px");
          };
        } else if (file.type) {
          // Create the generic icon for unknown type file.
          let type = "<i class='fa fa-file'></i>";

          // If the file is an audio file, replace the icon by an audio file icon.
          if (file.type.search(/audio/) != -1) {
            type = "<i class='fa fa-file-audio-o'></i>";
          }
          // If the file is an video file, replace the icon by an video file icon.
          else if (file.type.search(/video/) != -1) {
            type = "<i class='fa fa-file-video-o'></i>";
          }

          // Associated function to a ending load
          fReader.onloadend = function (e) {
            // Create the span tag for the file type.
            let span = $("<span>" + type + "</span>");

            // Span CSS.
            //span.css("font-size", "5em");

            // Append the span to its container and then the container to the
            // list of files.
            container.append(span);
            imagesList.append(container);

            // Apply left margin to first container of each row and right to last.
            imagesList
              .find(".imageuploadify-container:nth-child(" + boxesNb + "n+4)")
              .css("margin-left", marginSize + "px");
            imagesList
              .find(".imageuploadify-container:nth-child(" + boxesNb + "n+3)")
              .css("margin-right", marginSize + "px");
          };
        }

        // Delete the file from the list.
        deleteBtn.on("click", function () {
          //var parent = $(this).parent();
          //var index = parent.index("div.imageuploadify-container");
          //const dt = new DataTransfer()
          //const input = self;//document.getElementById('files')
          //const { files } = input
          //console.log(totalFiles, files);
          //for (let i = 0; i < files.length; i++) {
          //    const file = files[i]
          //    if (index !== i)
          //        dt.items.add(file) // here you exclude the file. thus removing it.
          //}

          //input.files = dt.files

          const dt = new DataTransfer();
          const input = self;
          $(this.parentElement).remove();
          for (let index = 0; totalFiles.length > index; ++index) {
            if (totalFiles[index].id === id) {
              totalFiles.splice(index, 1);
              break;
            }
          }
          for (index = 0; totalFiles.length > index; ++index) {
            let file = totalFiles[index].file;
            dt.items.add(file);
          }
          input.files = dt.files;
        });

        // Use the FileReader to read the content of a File.
        fReader.readAsDataURL(file);
      };

      const disableMouseEvents = () => {
        // Display the overlay and change the dragbox border color.
        overlay.css("display", "flex");
        dragbox.css("border-color", "#3AA0FF");

        // Disable pointer events to avoid miscapture dragexit children's events.
        button.css("pointer-events", "none");
        addMsg.css("pointer-events", "none");
        addIcon.css("pointer-events", "none");
        imagesList.css("pointer-events", "none");
      };

      const enableMouseEvents = () => {
        // Hide the overlay and put back the dragbox border color.
        overlay.css("display", "none");
        dragbox.css("border-color", "rgb(210, 210, 210)");

        // Enable back pointer events to capture click, hover...
        button.css("pointer-events", "initial");
        addMsg.css("pointer-events", "initial");
        addIcon.css("pointer-events", "initial");
        imagesList.css("pointer-events", "initial");
      };

      /** EVENTS  */
      // Change the color background of the button according to the mouse.
      button
        .mouseenter(function onMouseEnter(event) {
          button.css("background", "#3AA0FF").css("color", "white");
        })
        .mouseleave(function onMouseLeave() {
          button.css("background", "white").css("color", "#3AA0FF");
        });

      // When click on the button, simulate click on the original input.
      button.on("click", function onClick(event) {
        event.stopPropagation();
        event.preventDefault();
        $(self).click();
      });

      // Manage events to display an overlay when dragover files.
      dragbox.on("dragenter", function onDragenter(event) {
        event.stopPropagation();
        event.preventDefault();

        // Increment the counter.
        counter++;
        disableMouseEvents();
      });

      // Manage events to hide the overlay when dragout files.
      dragbox.on("dragleave", function onDragLeave(event) {
        event.stopPropagation();
        event.preventDefault();

        // Decrease the counter.
        counter--;

        // If the counter is equal to 0 (means that the files are entirely out
        // of the dragbox).
        if (counter === 0) {
          enableMouseEvents();
        }
      });

      // Manage events when dropping files.
      dragbox.on("drop", function onDrop(event) {
        event.stopPropagation();
        event.preventDefault();

        enableMouseEvents();
        // Retrieve the dragged files.
        const files = event.originalEvent.dataTransfer.files;
        //const files = this.files;
        let Allow_Only_Image =
          typeof AllowOnlyImage == "undefined" ? "" : AllowOnlyImage;
        if (Allow_Only_Image == "True") {
          let allowEidtImg = false;
          //for (let index = 0; index < files.length; ++index) {
          //    if ($image(files[index].type, files[index].name)) {
          //        allowEidtImg = true;
          //        break;
          //    }
          //}
          if (files.length == 1) {
            if ($image(files[0].type, files[0].name)) allowEidtImg = true;
          }
          if (allowEidtImg) {
            //$("div.avatar").remove();
            //$("div.imageuploadify-container").remove();
            //totalFiles = [];
            //counter = 0;
            if (IsShowdrawPhotoEditor) {
              if (files && files.length > 0) {
                let file = files[0];
                let done = function (url) {
                  drawPhotoEditor(url, file.name);
                };
                if (URL) {
                  done(URL.createObjectURL(file));
                } else if (FileReader) {
                  let reader = new FileReader();
                  reader.onload = function (e) {
                    done(reader.result);
                  };
                  reader.readAsDataURL(file);
                }
              }
            } else {
              queueFiles(files);
              IsShowdrawPhotoEditor = true;
            }
          } else {
            queueFiles(files);
          }
        } else {
          queueFiles(files);
        }
        // Read all files (to add them to the preview and push them to the files
        // list to submit).
        //let Allow_Only_Image = (typeof AllowOnlyImage == "undefined" ? "" : AllowOnlyImage);
        //if (Allow_Only_Image == "True") {
        //    if (files.length > 1) {
        //        DialogSweetAlert("error", "Allow only one file for upload.");
        //        return;
        //    }
        //    for (let index = 0; index < files.length; ++index) {
        //        if (!accept || compareMimeType(result, files[index].type, /(?:\.([^.]+))?$/.exec(files[index].name)[1])) {
        //            //ok
        //        }
        //        else {
        //            DialogSweetAlert("error", "File type not support for upload. Please upload only " + accept);
        //            return;
        //            break;
        //        }
        //    }
        //    $("div.avatar").remove();
        //    $("div.imageuploadify-container").remove();
        //    totalFiles = [];
        //    counter = 0;
        //    if (IsShowdrawPhotoEditor) {
        //        if (files && files.length > 0) {
        //            let file = files[0];
        //            let done = function (url) {
        //                drawPhotoEditor(url, file.name);
        //            };
        //            if (URL) {
        //                done(URL.createObjectURL(file));
        //            } else if (FileReader) {
        //                let reader = new FileReader();
        //                reader.onload = function (e) {
        //                    done(reader.result);
        //                };
        //                reader.readAsDataURL(file);
        //            }
        //        }
        //    }
        //    else {
        //        retrieveFiles(files);
        //        IsShowdrawPhotoEditor = true;
        //    }
        //}
        //else {
        //    retrieveFiles(files);
        //}
      });
      // Binding resize event to the window.
      $(window).bind("resize", function (e) {
        window.resizeEvt;
        $(window).resize(function () {
          // Delete the timeout as long as the window is still resizing.
          clearTimeout(window.resizeEvt);
          // Compute and change the margin according to the size of the window after
          // 0.5 seconds after resizing.
          window.resizeEvt = setTimeout(function () {
            // Compute the number of box that could fit in the dragbox and the
            // margin according to it.
            const width = dragbox.width();
            const boxesNb = Math.floor(width / 100);
            const marginSize = Math.floor(
              (width - boxesNb * 100) / (boxesNb + 1),
            );

            // Reset all margins of containers boxes.
            let containers = imagesList.find(".imageuploadify-container");
            for (let index = 0; index < containers.length; ++index) {
              $(containers[index]).css("margin-right", "0px");
              $(containers[index]).css("margin-left", marginSize + "px");
            }

            // Apply left margin to first container of each row and right to last.
            imagesList
              .find(".imageuploadify-container:nth-child(" + boxesNb + "n+4)")
              .css("margin-left", marginSize + "px");
            imagesList
              .find(".imageuploadify-container:nth-child(" + boxesNb + "n+3)")
              .css("margin-right", marginSize + "px");
          }, 500);
        });
      });

      // Detect when adding files through the dialog box to preview those files
      // and add them to the array.
      $(self).on("change", function onChange(e) {
        const files = this.files;
        let Allow_Only_Image =
          typeof AllowOnlyImage == "undefined" ? "" : AllowOnlyImage;
        if (Allow_Only_Image == "True") {
          let allowEidtImg = false;
          //for (let index = 0; index < files.length; ++index) {
          //    if ($image(files[index].type, files[index].name)) {
          //        allowEidtImg = true;
          //        break;
          //    }
          //}
          if (files.length == 1) {
            if ($image(files[0].type, files[0].name)) allowEidtImg = true;
          }
          if (allowEidtImg) {
            //$("div.avatar").remove();
            //$("div.imageuploadify-container").remove();
            //totalFiles = [];
            //counter = 0;
            if (IsShowdrawPhotoEditor) {
              if (files && files.length > 0) {
                let file = files[0];
                let done = function (url) {
                  drawPhotoEditor(url, file.name);
                };
                if (URL) {
                  done(URL.createObjectURL(file));
                } else if (FileReader) {
                  let reader = new FileReader();
                  reader.onload = function (e) {
                    done(reader.result);
                  };
                  reader.readAsDataURL(file);
                }
              }
            } else {
              queueFiles(files);
              IsShowdrawPhotoEditor = true;
            }
          } else {
            queueFiles(files);
          }
        } else {
          queueFiles(files);
        }
      });

      // When submitting the form.
      const submitUpload = (form) => {
        const inputs = form.querySelectorAll("input, textarea, select, button");
        const formData = new FormData();

        for (let index = 0; index < inputs.length; ++index) {
          const input = inputs[index];
          const type = (input.getAttribute("type") || "").toLowerCase();

          if (input.tagName === "SELECT" && input.hasAttribute("multiple")) {
            for (
              let optionIndex = 0;
              optionIndex < input.options.length;
              ++optionIndex
            ) {
              const option = input.options[optionIndex];
              if (option.selected) {
                formData.append(input.name, option.value);
              }
            }
          } else if (
            (type !== "checkbox" && type !== "radio") ||
            input.checked
          ) {
            formData.append(input.name, input.value);
          }
        }

        for (let index = 0; index < totalFiles.length; ++index) {
          formData.append(self.name, totalFiles[index].file);
        }

        const xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
          if (xhr.status === 200 && xhr.readyState === XMLHttpRequest.DONE) {
            window.location.replace(xhr.responseURL);
          }
        };

        xhr.open("POST", $(form).attr("action"), true);
        xhr.send(formData);
      };

      $(self)
        .closest("form")
        .on("submit", function (event) {
          event.stopPropagation();
          event.preventDefault();
          pendingFileProcessing.then(() => submitUpload(this));
          return false;
        });

      // Hide the original input.
      $(self).hide();
      // Insert the dragbox after the original hidden input.

      dragbox.insertAfter(this);
    });
    // Return "this" to ensure that chaining methods can be called.
    return this;
  };

  // Default configuraiton of the plugin.
  $.fn.imageuploadify.defaults = {};
})(jQuery, window, document);
