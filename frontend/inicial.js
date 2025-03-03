const criarHistoriaBtn = document.getElementById('criarHistoriaBtn');
const popup = document.getElementById('popup');
const fecharPopupBtn = document.getElementById('fecharPopup');
const salvarNomeBtn = document.getElementById('salvarNome');


criarHistoriaBtn.addEventListener('click', () => {
    popup.style.display = 'flex';
});


fecharPopupBtn.addEventListener('click', () => {
    popup.style.display = 'none';
});

/*Pop-up usuário*/ 
const perfilBtn = document.getElementById("open-user");
const popUser = document.getElementById("pop-user");
const fecharUser = document.querySelector(".fechar-user");

perfilBtn.addEventListener("click", () => {
    popUser.style.display = "block";
});

fecharUser.addEventListener("click", () => {
    popUser.style.display = "none";
});

window.addEventListener("click", (event) => {
    if (event.target === popUser) {
        popUser.style.display = "none";
    }
});
