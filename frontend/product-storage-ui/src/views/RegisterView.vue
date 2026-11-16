<script setup lang="ts">
import api from '@/api/api';
import type { RegisterRequest } from '@/models/auth';
import { ref } from 'vue';
import { useRouter } from 'vue-router';


const router = useRouter();

const fullname = ref<string>('');
const email = ref<string>('');
const password = ref<string>('');

async function register(){
    try{
        const request : RegisterRequest = {
            fullname: fullname.value,
            email: email.value,
            password: password.value
        };

        const response = await api.post('/Auth/register', request);
        localStorage.setItem('token', response.data.token);
        router.push('/login');
    }
    catch(error){
        console.error('Error registering user:', error);
        alert('An error occurred while registering. Please try again.');
    }
}

</script>

<template>
    <h1>Register</h1>
    <label for="fullname">Full Name:</label>
    <input type="text" v-model="fullname" id="fullname" placeholder="Full Name" required />
    <br><br>
    <label for="email">Email:</label>
    <input type="email" v-model="email" id="email" placeholder="Email" required />
    <br><br>
    <label for="password">Password:</label>
    <input type="password" v-model="password" id="password" placeholder="Password" required />
    <br><br>
    <button @click="register">Register</button>
    <br><br>
    <RouterLink to="/login">Already have an account? Login</RouterLink>
</template>

<style scoped></style>
