(function () {
    abp.helper ??= {};
    abp.helper.validateTicketPhoto = function ($input) {
        return abp.helper.validateFile($input, abp.helper.maxFileSizes.ticketPhoto);
    };

    abp.helper.validateInsurancePhoto = function ($input) {
        return abp.helper.validateFile($input, abp.helper.maxFileSizes.insuranceDocument);
    };

    abp.helper.validateFile = function ($input, maxSize) {
        let files = $input.get(0).files;

        if (!files.length) {
            abp.helper.formatAndShowValidationMessage('No file is selected.');
            return false;
        }

        let file = files[0];
        let type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
        if ('|jpg|jpeg|png|gif|pdf|'.indexOf(type) === -1) {
            abp.helper.formatAndShowValidationMessage('Invalid file type.');
            return false;
        }

        if (file.size > maxSize) {
            abp.helper.formatAndShowValidationMessage(`Size of the file exceeds allowed limits of ${maxSize / (1024 * 1024)} MB.`);
            return false;
        }

        return true;
    };

    abp.helper.pickTicketPhotoAsync = async function () {
        var file = await pickFileAsync({
            accept: abp.helper.fileTypes.imageOrPdf,
            maxSize: abp.helper.maxFileSizes.ticketPhoto,
        });
        return file;
    };

    abp.helper.pickInsuranceDocumentAsync = async function () {
        var file = await pickFileAsync({
            accept: abp.helper.fileTypes.imageOrPdf,
            maxSize: abp.helper.maxFileSizes.insuranceDocument,
        });
        return file;
    };

    /**
     * @typedef {Object} PickFileOptions
     * @property {string} [accept] - The file types that the file input should accept. For example, 'image/*' for images.
     * @property {number} [maxSize] - The maximum size of the file in bytes.
     * @property {"user"|"environment"|true} [capture] - The source of the file input. For example, 'user' for camera.
     */

    /**
     * @typedef {Object} PickFileResult
     * @property {string} [filename] - The filename of the picked file
     * @property {string} [fileBytesString] - Contents of the picked file as a dataURL string
     */

    /**
     * Shows file picker and returns the selected file.
     * @param {PickFileOptions} [options] - The options of the file picker.
     * @returns {Promise<PickFileResult>} The selected file.
     */
    async function pickFileAsync(options) {
        return new Promise((resolve, reject) => {
            let id = abp.helper.getUniqueElementId();
            var $input = $('<input type="file" class="upload d-none" />').attr('id', id);
            if (options.accept) {
                $input.attr('accept', options.accept);
            }
            if (options.capture) {
                let capture = options.capture === true ? 'environment' : options.capture;
                $input.attr('capture', capture);
            }

            let fileSelected = false;

            let rejectAndCleanup = (error) => {
                overlay?.remove();
                reject(new Error(error));
            }

            $input.change(function () {
                fileSelected = true;
                if (!abp.helper.validateFile($input, options.maxSize)) {
                    rejectAndCleanup('File size validation failed');
                    return;
                }
                const file = $input[0].files[0];
                const reader = new FileReader();

                reader.addEventListener("load", function () {
                    overlay?.remove();
                    resolve({
                        filename: file.name,
                        fileBytesString: reader.result,
                    });
                }, false);

                reader.addEventListener("error", function () {
                    abp.notify.error('Unable to read the file');
                    rejectAndCleanup('Unable to read the file');
                });

                reader.readAsDataURL(file);
            });

            var overlay = $('<div>').css({
                position: 'fixed',
                top: 0,
                left: 0,
                width: '100%',
                height: '100%',
                backgroundColor: 'rgba(0,0,0,0)',
                zIndex: 9999
            }).appendTo('body');

            overlay.on('mousemove click', () => {
                overlay.remove();
                if (!fileSelected) {
                    rejectAndCleanup('No file selected');
                }
            });

            $input.click();
        });
    }
    abp.helper.pickFileAsync = pickFileAsync;
})();
