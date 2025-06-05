plugins {
    kotlin("jvm") version "1.9.0"
    application
}

group = "com.example"
version = "1.0-SNAPSHOT"

repositories {
    mavenCentral()
}

dependencies {
    implementation("io.socket:socket.io-client:2.1.1")
    implementation("org.json:json:20240303")
    implementation(kotlin("stdlib"))
    implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:1.7.3")
}

application {
    mainClass.set("com.example.tscam.Main")
}

tasks.named<JavaExec>("run") {
    standardInput = System.`in`
}