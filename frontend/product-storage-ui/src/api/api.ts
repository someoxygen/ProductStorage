import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  headers:{
    'Content-Type': 'application/json',
  }
});

api.interceptors.request.use((config) =>{
    const token = localStorage.getItem('token');
    if(token){
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

api.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401 && error.config?.headers?.Authorization) {
            localStorage.removeItem('token');
            window.location.replace(`${import.meta.env.BASE_URL}login`);
        }
        return Promise.reject(error);
    }
);

export default api;
