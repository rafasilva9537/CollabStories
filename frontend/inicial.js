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
