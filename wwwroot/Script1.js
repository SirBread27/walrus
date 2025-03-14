// JavaScript source code
const container = document.querySelector(".container"),
    pwShowHide = document.querySelectorAll(".showHidePw"),
    pwFields = document.querySelectorAll(".password"),
    chooseSignUp = document.querySelector(".signup-link"),
    chooselogin = document.querySelector(".login-link"),
    signUp = document.querySelector(".signUp"),
    login = document.querySelector(".login");

        // show/hide password
        pwShowHide.forEach(eyeIcon => {
            eyeIcon.addEventListener("click", ()=>{
                pwFields.forEach(pwField => {
                    if (pwField.type === "password"){
                        pwField.type = "text";

                        pwShowHide.forEach(icon => {
                            icon.classList.replace("uil-eye-slash", "uil-eye");
                        })
                    } else {
                        pwField.type = "password";

                        pwShowHide.forEach(icon => {
                            icon.classList.replace("uil-eye", "uil-eye-slash");
                        })
                    }
                })
            })
        })

//sign up and login code

chooseSignUp.addEventListener("click", ( ) => {
    container.classList.add("active");
});
chooselogin.addEventListener("click", ( ) => {
    container.classList.remove("active");
});

const name_login = document.getElementById('name');
const password_login = document.getElementById('password-login');

const name_register = document.getElementById('name-register');
const email = document.getElementById('email');
const password_register_first = document.getElementById('password_register_first');
const password_register_second = document.getElementById('password_register_second');



//register

async function GoAfterRegistration() {
    
    if (password_register_first.value === password_register_second.value) {
        const resp= await fetch("/register?" + new URLSearchParams({
            login: name_register.value,
            password: password_register_first.value,
            email: email.value,
        }), {
            method: "POST",
            headers: { "Accept": "application/json" }
        });
        if (resp.ok) {
            localStorage.setItem("userdata", JSON.stringify(await resp.json()));
            document.location.href = "Chats.html";
        }
        else {
            console.log("mistake");
            document.location.href = "#zatemnenie-fetchMistake";
        }
    }
    else {
        document.location.href = "#zatemnenie-passwords";
    }
}

//login

//function GoAfterLogin() {

//    fetch("/login?" + new URLSearchParams({
//        login: name_login.value,
//        password: password_login.value,
//    }),
//        {
//            method: "GET",
//            headers:
//            {
//                "Accept": "application/json",
//                "Access-Control-Allow-Origin": "*"
//            }

//        })
//        .then(response => {
//            if (response.ok) {
//                console.log("a");
//                return response.json();
//            }
//            else { throw new Error("err") }
//        }).then(user => {
//            localStorage.setItem("userdata", user);
//            //document.location.href = "Chats.html";
//            console.log(user);
//        }).catch(err =>
//            {
//                document.location.href = "#zatemnenie-login";
//                console.log(err);
//            }
//        );
//}

async function GoAfterLogin() {

    resp = await fetch("/login?" + new URLSearchParams({
        login: name_login.value,
        password: password_login.value,
    }),
        {
            method: "GET",
            headers:
            {
                "Accept": "application/json",
                "Access-Control-Allow-Origin": "*"
            }

        });
    if (resp.ok)
    {
        localStorage.setItem("userdata", JSON.stringify(await resp.json()));
        document.location.href = "Chats.html";
    }
    else
    {
        document.location.href = "#zatemnenie-fetchMistake";
        console.log("suka blyat");
    }   
}
     

/*  
    const divt = document.getElementById('2');
    divt.addEventListener('click', function (event) {
        event.preventDefault();
      }
        document.location.href = "HTMLPage1.html";
    });*/