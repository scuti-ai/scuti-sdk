using System;
using UnityEngine;
using UnityEngine.UI;
using Scuti;

namespace Scuti.UI {
	public class AccountDetailsForm : Form<AccountDetailsForm.FormModel> {
		[Serializable]
		public class FormModel : Form.Model {
			public string FullName;
			public string Email;
			public Gender Gender;
			public string PhoneNumber;
			public string DateOfBirth;
		}

		[SerializeField] InputField fullNameInput;
		[SerializeField] InputField emailInput;
		//[SerializeField] Dropdown genderDropdown;
		[SerializeField] InputField phoneNumberInput;
		[SerializeField] InputField DOBInput;

        public override void Bind() {
            fullNameInput.onValueChanged.AddListener(value => Data.FullName = value);
            emailInput.onValueChanged.AddListener(value => Data.Email = value);
            phoneNumberInput.onValueChanged.AddListener(value => Data.PhoneNumber = value);
            DOBInput.onValueChanged.AddListener(value => Data.DateOfBirth = value);
        }

        public override void Refresh() {
            fullNameInput.text = Data.FullName;
            emailInput.text = Data.Email;
            phoneNumberInput.text = Data.PhoneNumber;
            DOBInput.text = Data.DateOfBirth;
        }

        public override FormModel GetDefaultDataObject() {
            return new FormModel();
        }
    }	
}

