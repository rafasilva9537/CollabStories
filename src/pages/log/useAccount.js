import { useState } from "react";
import { useNavigate } from "react-router-dom";

export function useRegister() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({
    displayName: "",
    username: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [usernameAvailable, setUsernameAvailable] = useState(true);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const checkUsernameAvailability = () => {
    const users = JSON.parse(localStorage.getItem("users") || "[]");
    const isAvailable = !users.some((user) => user.username === formData.username);
    setUsernameAvailable(isAvailable);
    return isAvailable;
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (name === "username" && value.length > 3) {
      checkUsernameAvailability();
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    setError("");
    setSuccess("");

    if (!formData.displayName || !formData.username || !formData.email || !formData.password) {
      setError("Todos os campos são obrigatórios");
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      setError("As senhas não coincidem");
      return;
    }

    if (formData.password.length < 6) {
      setError("A senha deve ter pelo menos 6 caracteres");
      return;
    }

    if (!checkUsernameAvailability()) {
      setError("Nome de usuário já está em uso");
      return;
    }

    try {
      const users = JSON.parse(localStorage.getItem("users") || "[]");
      if (users.some((user) => user.email === formData.email)) {
        setError("Este email já está cadastrado");
        return;
      }

      const newUser = {
        id: Date.now(),
        displayName: formData.displayName,
        username: formData.username,
        email: formData.email,
        password: formData.password,
      };

      users.push(newUser);
      localStorage.setItem("users", JSON.stringify(users));
      localStorage.setItem("currentUser", JSON.stringify(newUser));

      setSuccess("Conta criada com sucesso! Redirecionando...");
      setFormData({
        displayName: "",
        username: "",
        email: "",
        password: "",
        confirmPassword: "",
      });

      setTimeout(() => navigate("/home", { replace: true }), 2000);
    } catch {
      setError("Erro ao criar conta. Tente novamente.");
    }
  };

  return { formData, error, success, usernameAvailable, handleInputChange, handleSubmit };
}

export function useLogin() {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({ email: "", password: "" });
  const [error, setError] = useState("");

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    setError("");

    if (!formData.email || !formData.password) {
      setError("Email e senha são obrigatórios");
      return;
    }

    try {
      const users = JSON.parse(localStorage.getItem("users") || "[]");
      const user = users.find(
        (u) => u.email === formData.email && u.password === formData.password
      );

      if (user) {
        localStorage.setItem("currentUser", JSON.stringify(user));
        navigate("/home", { replace: true });
      } else {
        setError("Email ou senha incorretos");
      }
    } catch {
      setError("Erro ao fazer login. Tente novamente.");
    }
  };

  return { formData, error, handleInputChange, handleSubmit };
}