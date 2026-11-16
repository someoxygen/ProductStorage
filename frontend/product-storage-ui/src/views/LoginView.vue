<script setup lang="ts">
import api from '@/api/api';
import type { LoginRequest } from '@/models/auth';
import { ref } from 'vue';
import { useRouter } from 'vue-router';


const router = useRouter();

const email = ref<string>('');
const password = ref<string>('');

async function login(){
    try{
        const request : LoginRequest = {
            email: email.value,
            password: password.value
        };

        const response = await api.post('/Auth/login', request);
        localStorage.setItem('token', response.data.token);
        router.push('/products');
    }
    catch(error){
        console.error('Error logging in user:', error);
        alert('An error occurred while logging in. Please try again.');
    }
}

</script>

<template>
    <h1>Login</h1>
    <label for="email">Email:</label>
    <input type="email" v-model="email" id="email" placeholder="Email" required />
    <br><br>
    <label for="password">Password:</label>
    <input type="password" v-model="password" id="password" placeholder="Password" required />
    <br><br>
    <button @click="login">Login</button>
    <br><br>
    <RouterLink to="/register">Don't have an account? Register</RouterLink>
</template>

<style scoped></style>
