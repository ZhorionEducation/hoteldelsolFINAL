$(document).ready(function () {
    var numeroAcompanantesInput = $('#numeroAcompanantes');

    numeroAcompanantesInput.on('input', function () {
        var numAcompanantes = $(this).val();
        var huespedesContainer = $('#huespedesContainer');

        huespedesContainer.empty();

        if (numAcompanantes > 0 && numAcompanantes <= 5) {
            for (var i = 0; i < numAcompanantes; i++) {
                var nombreCampo = `Nombre del acompañante ${i + 1}`;
                var documentoCampo = `CC/CE del acompañante ${i + 1}`;

                var huespedForm = `
                    <div class="form-group">
                        <label for="Huespedes_${i}__Nombre" class="control-label">${nombreCampo}</label>
                        <input type="text" 
                               id="Huespedes_${i}__Nombre"
                               name="Huespedes[${i}].Nombre" 
                               class="form-control" 
                               data-val="true" 
                               data-val-required="El nombre del acompañante ${i + 1} es obligatorio." 
                               data-val-regex="El nombre del acompañante ${i + 1} debe contener solo letras y espacios (máximo 20 caracteres)."
                               data-val-regex-pattern="^[a-zA-Z\\s]{1,20}$">
                        <span class="text-danger field-validation-valid" data-valmsg-for="Huespedes[${i}].Nombre" data-valmsg-replace="true"></span>
                    </div>
                    <div class="form-group">
                        <label for="Huespedes_${i}__DocumentoIdentidad" class="control-label">${documentoCampo}</label>
                        <input type="text" 
                               id="Huespedes_${i}__DocumentoIdentidad"
                               name="Huespedes[${i}].DocumentoIdentidad" 
                               class="form-control" 
                               data-val="true" 
                               data-val-required="La CC/CE del acompañante ${i + 1} es obligatorio." 
                               data-val-regex="LA CC/CE del acompañante ${i + 1} debe contener exactamente de 7 a 10 dígitos numéricos."
                               data-val-regex-pattern="^\\d{7,10}$"
                               data-val-unique-document="El documento de identidad debe ser único.">
                        <span class="text-danger field-validation-valid" data-valmsg-for="Huespedes[${i}].DocumentoIdentidad" data-valmsg-replace="true"></span>
                    </div>
                `;
                huespedesContainer.append(huespedForm);
            }

            // Destruir y reiniciar la validación
            var form = huespedesContainer.closest('form');
            form.removeData('validator');
            form.removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(form);

            // Validación personalizada para documentos únicos
            $.validator.addMethod("uniqueDocument", function (value, element) {
                var isUnique = true;
                var documentValues = [];
                $('input[name^="Huespedes"][name$="DocumentoIdentidad"]').each(function () {
                    if ($(this).val() !== "" && documentValues.includes($(this).val())) {
                        isUnique = false;
                        return false;
                    }
                    documentValues.push($(this).val());
                });
                return isUnique;
            }, "El documento de identidad debe ser único.");

            $('input[name^="Huespedes"][name$="DocumentoIdentidad"]').each(function () {
                $(this).rules("add", {
                    uniqueDocument: true
                });
            });
        }
    });
});